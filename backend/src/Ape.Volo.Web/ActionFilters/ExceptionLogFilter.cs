using Ape.Volo.Shared.Exceptions;
using System.ComponentModel;
using Ape.Volo.Web.Logging;
using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Logging;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using IP2Region.Net.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Shyjus.BrowserDetection;
using StackExchange.Profiling;
using ExceptionHelper = Ape.Volo.Web.Logging.ExceptionHelper;
using LogLevel = Ape.Volo.Shared.Enums.LogLevel;
using MiniProfiler = StackExchange.Profiling.MiniProfiler;

// 消歧义：与 Ape.Volo.Common.Helper.ExceptionHelper 重名，固定引用日志格式化器
namespace Ape.Volo.Web.ActionFilters;

/// <summary>
/// 异常日志过滤器
/// </summary>
public class ExceptionLogFilter : IAsyncExceptionFilter
{
    private readonly ILogQueue<ExceptionLogDto> _exceptionLogQueue;
    private readonly ISettingService _settingService;
    private readonly IBrowserDetector _browserDetector;
    private readonly ILogger<ExceptionLogFilter> _logger;
    private readonly ISearcher _ipSearcher;
    private readonly ICurrentUser _currentUser;
    private readonly MiddlewareOptions _middlewareOptions;
    private readonly IFallbackLogWriter _fallbackLogWriter;

    /// <summary>
    /// 初始化 <see cref="ExceptionLogFilter"/> 的新实例。
    /// </summary>
    /// <param name="exceptionLogQueue">exceptionLogQueue</param>
    /// <param name="searcher">searcher</param>
    /// <param name="settingService">settingService</param>
    /// <param name="browserDetector">browserDetector</param>
    /// <param name="logger">logger</param>
    /// <param name="currentUser">当前请求用户</param>
    /// <param name="middlewareOptions">中间件配置</param>
    /// <param name="fallbackLogWriter">独立兜底输出</param>

    public ExceptionLogFilter(ILogQueue<ExceptionLogDto> exceptionLogQueue, ISearcher searcher,
        ISettingService settingService, IBrowserDetector browserDetector,
        ILogger<ExceptionLogFilter> logger, ICurrentUser currentUser, IOptions<MiddlewareOptions> middlewareOptions,
        IFallbackLogWriter fallbackLogWriter)
    {
        _exceptionLogQueue = exceptionLogQueue;
        _settingService = settingService;
        _browserDetector = browserDetector;
        _logger = logger;
        _ipSearcher = searcher;
        _currentUser = currentUser;
        _middlewareOptions = middlewareOptions.Value;
        _fallbackLogWriter = fallbackLogWriter;
    }

    /// <summary>
    /// 捕获 Action 抛出的异常：包装为统一响应体，并按配置写入异常日志
    /// </summary>
    /// <param name="context">异常上下文</param>
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var exceptionType = context.Exception.GetType();
        var statusCode = StatusCodes.Status500InternalServerError;
        //自定义全局异常
        //if (exceptionType == typeof(ApevovoException))
        //{
        //  var ex = (ApevovoException)context.Exception;
        // statusCode = ex.StatusCode;
        // }
        if (exceptionType == typeof(BadRequestException)) //错误请求 无法处理
        {
            statusCode = StatusCodes.Status400BadRequest;
        }

        string throwMsg = context.Exception.Message; //错误信息
        var clientMessage = statusCode == StatusCodes.Status400BadRequest ? throwMsg : "服务器内部错误";
        var actionError = new ActionError { Errors = new Dictionary<string, string>() };
        context.Result = new ContentResult
        {
            Content = new ActionResultVm
            {
                Status = statusCode,
                ActionError = actionError,
                Message = clientMessage,
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Path = context.HttpContext.Request.Path.Value?.ToLower() ?? ""
            }.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = statusCode
        };
        try
        {
            if (_middlewareOptions.MiniProfiler)
            {
                MiniProfiler.Current?.CustomTiming("Errors：", throwMsg);
            }
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var ipAddress = _ipSearcher.Search(remoteIp);
            var arguments = await Requests.RequestParameterReader.ReadAsync(context.HttpContext.Request, context.HttpContext.RequestAborted);
            //记录日志
            _logger.LogError(ExceptionHelper.ErrorFormat(context.HttpContext, remoteIp, ipAddress, context.Exception,
                _currentUser.Account,
                _browserDetector.Browser?.OS, _browserDetector.Browser?.DeviceType, _browserDetector.Browser?.Name,
                _browserDetector.Browser?.Version, SensitiveDataRedactor.Serialize(arguments)));


            var saveDb = await _settingService.GetSettingValue<bool>("IsExceptionLogSaveDB", context.HttpContext.RequestAborted);
            // 400 是预期内的输入/业务错误，只留文件日志，不写异常日志表——
            // 否则任何客户端都能用非法参数把 sys_exception_log 灌满
            if (saveDb && exceptionType != typeof(DemoRequestException) &&
                exceptionType != typeof(BadRequestException))
            {
                //记录日志到数据库
                var log = CreateLog(context, arguments);
                if (log.IsNotNull())
                {
                    //写入异步队列，由后台服务批量落库
                    if (!_exceptionLogQueue.TryEnqueue(log))
                    {
                        _logger.LogWarning("异常日志队列已满，本次日志被丢弃（累计丢弃 {Dropped} 条）",
                            _exceptionLogQueue.DroppedCount);
                    }
                }
            }
        }
        catch (Exception e)
        {
            // 不依赖可能已失败的 IP、浏览器或数据库服务，同时保留原始业务异常。
            await _fallbackLogWriter.WriteAsync($"异常日志处理失败：{context.HttpContext.Request.Path}{Environment.NewLine}{e}" +
                                               $"{Environment.NewLine}原始异常：{context.Exception}");
        }
    }

    /// <summary>
    /// 创建异常对象
    /// </summary>
    /// <param name="context"></param>
    /// <param name="arguments">已异步读取的请求参数</param>
    /// <returns></returns>
    private ExceptionLogDto CreateLog(ExceptionContext context, Dictionary<string, object> arguments)
    {
        ExceptionLogDto? log = null;
        try
        {
            var routeValues = context.ActionDescriptor.RouteValues;
            var httpContext = context.HttpContext;
            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var descriptionAttribute = ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo
                .GetCustomAttributes(typeof(DescriptionAttribute), true)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault();
            var descriptionValue = descriptionAttribute != null ? descriptionAttribute.Description : "";
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
            log = new ExceptionLogDto
            {
                Id = IdHelper.NextId(),
                CreatorId = _currentUser.Id > 0 ? _currentUser.Id : null,
                CreatorName = _currentUser.Account,
                CreationTime = DateTime.Now,
                Area = routeValues["area"].IsNullOrEmpty() ? "" : routeValues["area"]!,
                Controller = routeValues["controller"] ?? "",
                Action = routeValues["action"] ?? "",
                Method = httpContext.Request.Method,
                Description = descriptionValue,
                RequestUrl = httpContext.Request.Path,
                RequestParameters = SensitiveDataRedactor.Serialize(arguments),
                ExceptionMessage = context.Exception.Message,
                ExceptionMessageFull = Common.Helper.ExceptionHelper.GetExceptionAllMsg(context.Exception),
                ExceptionStack = context.Exception.StackTrace ?? "",
                RequestIp = remoteIp,
                IpAddress = _ipSearcher.Search(remoteIp) ?? "",
                UserAgent = userAgent,
                LogLevel = LogLevel.Error,
                OperatingSystem = _browserDetector.Browser?.OS ?? "",
                DeviceType = _browserDetector.Browser?.DeviceType ?? "",
                BrowserName = _browserDetector.Browser?.Name ?? "",
                Version = _browserDetector.Browser?.Version ?? ""
            };
        }
        catch
        {
            // ignored
        }

        return log ?? new ExceptionLogDto();
    }
}
