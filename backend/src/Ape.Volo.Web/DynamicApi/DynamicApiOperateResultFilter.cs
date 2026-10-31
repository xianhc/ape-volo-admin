using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 动态接口业务失败过滤器：<c>OperateResult.IsSuccess = false</c> 时返回 400 与统一响应体，
/// 与控制器里 <c>Ok(OperateResult)</c> 的行为保持一致
/// </summary>
public sealed class DynamicApiOperateResultFilter : IAlwaysRunResultFilter
{
    /// <summary>
    /// 把失败的 <see cref="OperateResult"/> 换成 400 响应
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Result is not ObjectResult { Value: OperateResult { IsSuccess: false } result })
        {
            return;
        }

        context.Result = new ContentResult
        {
            Content = new ActionResultVm
            {
                Status = StatusCodes.Status400BadRequest,
                ActionError = new ActionError(),
                Message = result.Message,
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Path = context.HttpContext.Request.Path.Value?.ToLower()!
            }.ToJson(),
            ContentType = "application/json; charset=utf-8",
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    /// <summary>
    /// 结果执行完毕后无需处理
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
