using Ape.Volo.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.ActionFilters;

/// <summary>
/// 过滤器基类，提供统一的 JSON 返回能力
/// </summary>
public class BaseActionFilter : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Action 执行之前调用，可在子类中重写以拦截请求
    /// </summary>
    /// <param name="context">Action 执行上下文</param>
    public virtual async Task OnActionExecuting(ActionExecutingContext context)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Action 执行之后调用，可在子类中重写以处理结果
    /// </summary>
    /// <param name="context">Action 已执行上下文</param>
    public virtual async Task OnActionExecuted(ActionExecutedContext context)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// 过滤器主流程：先执行 <see cref="OnActionExecuting"/>，未被短路时继续执行 Action 并回调 <see cref="OnActionExecuted"/>
    /// </summary>
    /// <param name="context">Action 执行上下文</param>
    /// <param name="next">后续管道委托</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await OnActionExecuting(context);
        if (context.Result == null)
        {
            var nextContext = await next();
            await OnActionExecuted(nextContext);
        }
    }

    /// <summary>
    /// 返回JSON
    /// </summary>
    /// <param name="vm"></param>
    /// <param name="context">当前调用的请求上下文</param>
    /// <returns></returns>
    public static ContentResult JsonContent(HttpContext context, ActionResultVm vm)
    {
        return new ContentResult
        {
            Content = new ActionResultVm
            {
                Status = vm.Status,
                Message = vm.Message,
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Path = context.Request.Path.Value?.ToLower() ?? ""
            }.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = vm.Status
        };
    }

    /// <summary>
    /// 返回错误
    /// </summary>
    /// <param name="msg">错误提示</param>
    /// <param name="context">当前调用的请求上下文</param>
    /// <returns></returns>
    public static ContentResult Error(HttpContext context, string msg)
    {
        var vm = new ActionResultVm
        {
            Status = StatusCodes.Status400BadRequest,
            Message = msg
        };

        return JsonContent(context, vm);
    }

    /// <summary>
    /// 返回错误
    /// </summary>
    /// <param name="msg">错误提示</param>
    /// <param name="errorCode">错误代码</param>
    /// <param name="context">当前调用的请求上下文</param>
    /// <returns></returns>
    public static ContentResult Error(HttpContext context, string msg, int errorCode)
    {
        var vm = new ActionResultVm
        {
            Status = errorCode,
            Message = msg
        };

        return JsonContent(context, vm);
    }
}
