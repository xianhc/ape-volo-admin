using Ape.Volo.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.ActionFilters;

/// <summary>
/// 演示环境过滤器，拦截除查询外的写操作请求
/// </summary>
public class DemoFilter : IAsyncActionFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly string[] SourceArray = ["/auth/login","/auth/logout","/auth/refresh-token"];

    /// <summary>
    /// 初始化 <see cref="DemoFilter"/> 的新实例。
    /// </summary>
    /// <param name="httpContextAccessor">httpContextAccessor</param>
    public DemoFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 演示环境禁止除登录、注销、刷新令牌及查询之外的请求
    /// </summary>
    /// <param name="context">Action 执行上下文</param>
    /// <param name="next">后续管道委托</param>
    /// <exception cref="DemoRequestException">非只读请求时抛出，提示禁止编辑数据</exception>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = _httpContextAccessor?.HttpContext?.Request.Method.ToUpper();
        var reqUrl = _httpContextAccessor?.HttpContext?.Request.Path.Value?.ToLower();
        if (reqUrl != null && ((method != "GET" && !SourceArray.Contains(reqUrl)) || reqUrl.Contains("download")))
        {
            throw new DemoRequestException("谢谢您的参与，为了大家更好的体验，禁止编辑数据！");
        }
        await next();
    }
}
