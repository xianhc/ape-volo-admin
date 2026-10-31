using Ape.Volo.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 动态接口统一模型校验，在调用应用服务前按原控制器格式返回参数错误
/// </summary>
public sealed class DynamicApiValidationFilter : IActionFilter, IOrderedFilter
{
    /// <summary>
    /// 与 MVC 自动模型校验保持相同优先级，先于普通业务动作过滤器执行
    /// </summary>
    public int Order => -2000;

    /// <summary>
    /// 模型绑定或数据注解校验失败时短路请求，不执行应用服务
    /// </summary>
    /// <param name="context">动作执行上下文</param>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ModelState.IsValid) return;

        var actionError = context.ModelState.GetErrors();
        context.Result = new ContentResult
        {
            Content = new ActionResultVm
            {
                Status = StatusCodes.Status400BadRequest,
                ActionError = actionError,
                Message = actionError.GetFirstError(),
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Path = context.HttpContext.Request.Path.Value?.ToLower() ?? string.Empty
            }.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    /// <summary>
    /// 校验通过后的结果继续由原有结果过滤器处理
    /// </summary>
    /// <param name="context">动作执行完成上下文</param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
