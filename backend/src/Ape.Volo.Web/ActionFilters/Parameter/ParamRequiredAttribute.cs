using Ape.Volo.Common.Extensions;
using Ape.Volo.Web.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.ActionFilters.Parameter;

/// <summary>
/// 异步读取请求参数并校验必填项，不在过滤器实例保存请求状态。
/// </summary>
/// <param name="parameters">必填参数名集合</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class ParamRequiredAttribute(params string[] parameters) : Attribute, IAsyncActionFilter
{
    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var values = await RequestParameterReader.ReadAsync(context.HttpContext.Request, context.HttpContext.RequestAborted);
        var missing = parameters.Where(key => !values.TryGetValue(key, out var value) || value.IsNullOrEmpty()).ToArray();
        if (missing.Length == 0) { await next(); return; }
        context.Result = new ContentResult
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentType = "application/json;charset=utf-8",
            Content = new ActionResultVm
            {
                Status = StatusCodes.Status400BadRequest,
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Message = $"{string.Join(",", missing)} 是必填项",
                Path = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? ""
            }.ToJson()
        };
    }
}
