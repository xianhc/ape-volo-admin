using System.ComponentModel;
using System.Diagnostics;
using Ape.Volo.Web.Logging;
using Ape.Volo.Entity.Log;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Settings;
using Ape.Volo.Application.Contracts.Abstractions;
using Ape.Volo.Application.Contracts.Abstractions.Logging;
using Ape.Volo.Application.Contracts.System.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Shyjus.BrowserDetection;
namespace Ape.Volo.Web.ActionFilters;

/// <summary>
/// 操作日志过滤器
/// </summary>
public class OperateLogFilter : IAsyncActionFilter
{
    private readonly ILogQueue<OperateLog> _operateLogQueue;
    private readonly ISettingService _settingService;
    private readonly IBrowserDetector _browserDetector;
    private readonly IIpLocationService _ipSearcher;
    private readonly ILogger<OperateLogFilter> _logger;
    private readonly ICurrentUser _currentUser;
    private readonly IFallbackLogWriter _fallbackLogWriter;
    private readonly IIdGenerator _idGenerator;

    /// <summary>
    /// 初始化 <see cref="OperateLogFilter"/> 的新实例。
    /// </summary>
    /// <param name="operateLogQueue">operateLogQueue</param>
    /// <param name="searcher">searcher</param>
    /// <param name="settingService">settingService</param>
    /// <param name="browserDetector">browserDetector</param>
    /// <param name="logger">logger</param>
    /// <param name="currentUser">当前请求用户</param>
    /// <param name="fallbackLogWriter">独立兜底输出</param>
    /// <param name="idGenerator">唯一 ID 生成器</param>

    public OperateLogFilter(ILogQueue<OperateLog> operateLogQueue, IIpLocationService searcher,
        ISettingService settingService, IBrowserDetector browserDetector,
        ILogger<OperateLogFilter> logger, ICurrentUser currentUser, IFallbackLogWriter fallbackLogWriter,
        IIdGenerator idGenerator)
    {
        _operateLogQueue = operateLogQueue;
        _settingService = settingService;
        _browserDetector = browserDetector;
        _ipSearcher = searcher;
        _logger = logger;
        _currentUser = currentUser;
        _fallbackLogWriter = fallbackLogWriter;
        _idGenerator = idGenerator;
    }

    /// <summary>
    /// 过滤器入口，记录操作耗时与响应内容后写入操作日志队列
    /// </summary>
    /// <param name="context">Action 执行上下文</param>
    /// <param name="next">后续管道委托</param>
    /// <returns>异步任务</returns>
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        return Execute(context, next);
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    private async Task Execute(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var resultContext = await next();
        sw.Stop();

        try
        {
            var action = (ControllerActionDescriptor)context.ActionDescriptor;
            if (action.MethodInfo.IsDefined(typeof(NotOperateAttribute), false))
            {
                return;
            }

            //执行结果
            //var action = context.ActionDescriptor as ControllerActionDescriptor;
            //var isTrue = action.MethodInfo.IsDefined(typeof(DescriptionAttribute), false);
            var saveDb = await _settingService.GetSettingValue<bool>(
                SettingNames.OperateLogSaveToDatabase, context.HttpContext.RequestAborted);
            if (saveDb && resultContext.Result.IsNotNull())
            {
                var operateLog = await CreateOperateLogAsync(context);
                operateLog.ResponseData = resultContext.Result switch
                {
                    ContentResult contentResult => SensitiveDataRedactor.RedactJson(contentResult.Content!),
                    NoContentResult okResult => okResult.ToJson(),
                    OkObjectResult okResult when okResult.Value != null =>
                        SensitiveDataRedactor.Serialize(okResult.Value),
                    FileContentResult fileContentResult => GetFileContentResult(fileContentResult),
                    ObjectResult objectResult when objectResult.Value != null =>
                        SensitiveDataRedactor.Serialize(objectResult.Value),
                    _ => string.Empty // 处理其他未知类型
                };


                //用时
                operateLog.ExecutionDuration = sw.ElapsedMilliseconds;

                //写入异步队列，由后台服务批量落库
                if (!_operateLogQueue.TryEnqueue(operateLog))
                {
                    _logger.LogWarning("操作日志队列已满，本次日志被丢弃（累计丢弃 {Dropped} 条）",
                        _operateLogQueue.DroppedCount);
                }
            }
        }
        catch (Exception ex)
        {
            await _fallbackLogWriter.WriteAsync($"操作日志处理失败：{context.HttpContext.Request.Path}{Environment.NewLine}{ex}");
        }
    }

    /// <summary>
    /// 创建审计对象
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task<OperateLog> CreateOperateLogAsync(ActionExecutingContext context)
    {
        var routeValues = context.ActionDescriptor.RouteValues;

        var httpContext = context.HttpContext;
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var arguments = await Requests.RequestParameterReader.ReadAsync(httpContext.Request, httpContext.RequestAborted);
        var descriptionAttribute = ((ControllerActionDescriptor)context.ActionDescriptor).MethodInfo
            .GetCustomAttributes(typeof(DescriptionAttribute), true)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault();
        var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
        var descriptionValue = descriptionAttribute != null ? descriptionAttribute.Description : "";
        var operateLog = new OperateLog
        {
            Id = _idGenerator.NewId(),
            CreatorId = _currentUser.Id > 0 ? _currentUser.Id : null,
            CreatorName = _currentUser.Account,
            CreationTime = DateTime.Now,
            Area = routeValues["area"] ?? string.Empty,
            Controller = routeValues["controller"] ?? "",
            Action = routeValues["action"] ?? "",
            Method = httpContext.Request.Method,
            Description = descriptionValue,
            RequestUrl = httpContext.Request.Path,
            RequestParameters = SensitiveDataRedactor.Serialize(arguments),
            RequestIp = remoteIp,
            IpAddress = _ipSearcher.Search(remoteIp).ToString(),
            UserAgent = userAgent,
            OperatingSystem = _browserDetector.Browser?.OS ?? "",
            DeviceType = _browserDetector.Browser?.DeviceType ?? "",
            BrowserName = _browserDetector.Browser?.Name ?? "",
            Version = _browserDetector.Browser?.Version ?? ""
        };


        var reqUrl = httpContext.Request.Path.Value?.ToLower();
        if (reqUrl is "/api/auth/login")
        {
            var (_, value) = arguments.SingleOrDefault(k => k.Key == "username");
            if (!value.IsNullOrEmpty())
            {
                operateLog.CreatorName = value.ToString()!;
            }
        }

        return operateLog;
    }

    private string GetFileContentResult(FileContentResult fileContentResult)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hashBytes = md5.ComputeHash(fileContentResult.FileContents);
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return new
        {
            FileName = fileContentResult.FileDownloadName,
            FileSize = fileContentResult.FileContents.Length,
            ContentType = fileContentResult.ContentType,
            FileHash = hashString
        }.ToJson();
    }
}
