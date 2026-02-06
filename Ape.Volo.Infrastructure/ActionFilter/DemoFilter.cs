using Ape.Volo.Common.Exception;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Infrastructure.ActionFilter;

public class DemoFilter : IAsyncActionFilter
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly string[] SourceArray = ["/auth/login","/auth/logout","/auth/refreshtoken"];

    public DemoFilter(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

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
