using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers;

/// <summary>
/// 健康检测
/// </summary>
[Route("/health", Order = 999)]
public class HealthCheckController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    [NotOperate]
    [Route("index")]
    public Task<ActionResult> Index()
    {
        return Task.FromResult(Ok(new OperateResult { IsSuccess = true }));
    }
}