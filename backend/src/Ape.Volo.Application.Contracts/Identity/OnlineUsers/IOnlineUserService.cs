using Ape.Volo.Application.Contracts.Identity.Auth;
using Ape.Volo.Application.Contracts.Identity.Users.Models;
using Ape.Volo.Shared.Model;
using Ape.Volo.Shared.WebApp;

namespace Ape.Volo.Application.Contracts.Identity.OnlineUsers;

/// <summary>
/// 在线用户接口
/// </summary>
public interface IOnlineUserService
{
    #region 基础接口

    /// <summary>
    /// 保存在线用户
    /// </summary>
    /// <param name="userVo"></param>
    /// <param name="ct">取消令牌</param>
    Task<LoginUserInfo> SaveLoginUserAsync(UserVo userVo, CancellationToken ct = default);

    /// <summary>
    /// jwt用户信息
    /// </summary>
    /// <param name="userVo"></param>
    /// <param name="authCodeList"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<JwtUserVo> CreateJwtUserAsync(UserVo userVo, List<string> authCodeList, CancellationToken ct = default);


    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<LoginUserInfo>> GetListAsync(PagedAndSortedResultRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// 强退
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task DropOutAsync(HashSet<string> ids, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(CancellationToken ct = default);

    #endregion
}
