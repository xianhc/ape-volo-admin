using Ape.Volo.Application.Contracts.SystemManagement.Settings.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.SystemManagement.Settings;

/// <summary>
/// 全局设置接口
/// </summary>
public interface ISettingService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateSettingDto createUpdateSettingDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateSettingDto createUpdateSettingDto, CancellationToken ct = default);

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
    /// <param name="parameterQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<SettingVo>> GetListAsync(ParameterQueryCriteria parameterQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="parameterQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(ParameterQueryCriteria parameterQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 根据名称查询
    /// </summary>
    /// <param name="settingName"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<T?> GetSettingValue<T>(string settingName, CancellationToken ct = default);

    #endregion
}
