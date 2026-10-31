using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.ActionFilters;

/// <summary>
/// 将应用服务授权异常转换为 401/403；预期的拒绝不进入服务器异常日志处理。
/// </summary>
public sealed class ServiceAuthorizationExceptionFilter : IExceptionFilter, IOrderedFilter
{
    /// <inheritdoc />
    public int Order => int.MaxValue;

    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ServiceAuthorizationException exception) return;
        var status = exception.IsAuthenticated ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized;
        if (!exception.IsAuthenticated) context.HttpContext.Response.Headers.WWWAuthenticate = "Bearer";
        context.Result = new ContentResult
        {
            StatusCode = status,
            ContentType = "application/json; charset=utf-8",
            Content = new ActionResultVm
            {
                Status = status,
                Message = exception.Message,
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? ""
            }.ToJson()
        };
        context.ExceptionHandled = true;
    }
}
