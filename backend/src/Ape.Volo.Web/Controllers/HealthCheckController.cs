using Ape.Volo.Web.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Web.Controllers;

/// <summary>
/// 健康检测
/// </summary>
[Route("/health", Order = 999)]
public class HealthCheckController : BaseApiController
{
    /// <summary>
    /// 健康检测，用于探活与负载均衡检查
    /// </summary>
    /// <returns>固定返回成功标记</returns>
    [HttpGet]
    [AllowAnonymous]
    [NotOperate]
    [Route("index")]
    public Task<ActionResult> Index()
    {
        return Task.FromResult(Ok(new OperateResult { IsSuccess = true }));
    }
}