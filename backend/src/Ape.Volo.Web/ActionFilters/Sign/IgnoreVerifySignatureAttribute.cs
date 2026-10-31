using Microsoft.AspNetCore.Mvc.Filters;

namespace Ape.Volo.Web.ActionFilters.Sign;

/// <summary>
/// 标记接口跳过签名校验
/// </summary>
public class IgnoreVerifySignatureAttribute : BaseActionFilter
{
    /// <summary>
    /// Action执行之前执行
    /// </summary>
    /// <param name="filterContext"></param>
    public override async Task OnActionExecuting(ActionExecutingContext filterContext)
    {
        await Task.CompletedTask;
    }
}
