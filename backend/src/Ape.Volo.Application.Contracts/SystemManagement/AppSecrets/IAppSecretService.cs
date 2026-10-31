using Ape.Volo.Application.Contracts.SystemManagement.AppSecrets.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.SystemManagement.AppSecrets;

/// <summary>
/// 应用秘钥
/// </summary>
public interface IAppSecretService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateAppSecretDto createUpdateAppSecretDto, CancellationToken ct = default);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<AppSecretVo>> GetListAsync(AppSecretQueryCriteria appsecretQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(AppSecretQueryCriteria appsecretQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 按 AppId 查询应用密钥
    /// </summary>
    /// <param name="appId">应用 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>应用密钥，不存在时返回 null</returns>
    [RemoteService(false)]
    Task<AppSecretVo?> QueryByAppIdAsync(string appId, CancellationToken ct = default);

    #endregion
}
