using System.ComponentModel;
using Ape.Volo.Web.Controllers.Base;
using Ape.Volo.Shared.Model;
using Ape.Volo.Shared.WebApp;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Web.Controllers.Identity;

/// <summary>
/// 在线用户管理，显式保留既有 HTTP 路由、Redis 限制和导出响应
/// </summary>
[Area("在线用户管理")]
[Route("/api/online", Order = 15)]
public class OnlineUserController : BaseApiController
{
    private readonly IOnlineUserService _onlineUserService;
    private readonly SystemOptions _systemOptions;

    /// <summary>
    /// 初始化在线用户控制器
    /// </summary>
    /// <param name="onlineUserService">内部在线用户服务</param>
    /// <param name="systemOptions">系统配置</param>
    public OnlineUserController(IOnlineUserService onlineUserService, IOptions<SystemOptions> systemOptions)
    {
        _onlineUserService = onlineUserService;
        _systemOptions = systemOptions.Value;
    }

    /// <summary>
    /// 在线用户列表
    /// </summary>
    /// <param name="request">分页及排序参数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>在线用户分页结果</returns>
    [HttpGet]
    [Route("get-list")]
    [Description("执行查询")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResultDto<LoginUserInfo>))]
    public async Task<ActionResult> GetList(PagedAndSortedResultRequestDto request, CancellationToken ct)
    {
        if (!_systemOptions.UseRedisCache)
            return Error("在线用户功能仅支持 Redis，请启用 UseRedisCache。");

        if (!ModelState.IsValid)
            return Error(ModelState.GetErrors());

        return JsonContent(await _onlineUserService.GetListAsync(request, ct));
    }

    /// <summary>
    /// 强制登出用户
    /// </summary>
    /// <param name="request">待撤销的会话标识集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>强退操作结果</returns>
    [HttpDelete]
    [Route("out")]
    [Description("强退用户")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> DropOut([FromBody] DropOutSessionsRequest request, CancellationToken ct)
    {
        if (!_systemOptions.UseRedisCache)
            return Error("在线用户功能仅支持 Redis，请启用 UseRedisCache。");

        if (!ModelState.IsValid)
            return Error(ModelState.GetErrors());

        await _onlineUserService.DropOutAsync(request.IdArray, ct);
        return Ok(OperateResult.Success());
    }

    /// <summary>
    /// 导出在线用户
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>在线用户导出文件</returns>
    [HttpGet]
    [Description("执行导出")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(CancellationToken ct = default)
    {
        if (!_systemOptions.UseRedisCache)
            return Error("在线用户功能仅支持 Redis，请启用 UseRedisCache。");

        return ExportExcel(await _onlineUserService.DownloadAsync(ct), "在线用户");
    }
}
