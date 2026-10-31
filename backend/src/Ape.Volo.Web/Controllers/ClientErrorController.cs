using Ape.Volo.Application.Contracts.Diagnostics;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Web.Controllers;

/// <summary>接收已登录客户端的有限错误诊断。</summary>
[ApiController]
[Route("api/client-errors")]
public sealed class ClientErrorController(ILogger<ClientErrorController> logger) : BaseApiController
{
    /// <summary>将经过字段限制的诊断摘要写入现有日志通道。</summary>
    /// <param name="report">客户端诊断。</param>
    /// <param name="ct">请求取消令牌。</param>
    /// <returns>成功接收后返回 204。</returns>
    [HttpPost]
    [NotOperate]
    [RequestSizeLimit(4096)]
    public IActionResult Report([FromBody] ClientErrorReportDto report, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        logger.LogWarning("客户端错误 {Code} {ErrorName} Route={RouteName} Build={Build} RequestId={RequestId}",
            report.Code, report.ErrorName, report.RouteName, report.Build, report.RequestId);
        return NoContent();
    }
}
