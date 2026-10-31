using Ape.Volo.Application.Contracts.Organization.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Organization;

/// <summary>
/// 岗位接口
/// </summary>
public interface IPositionService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdatePositionDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdatePositionDto createUpdatePositionDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdatePositionDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdatePositionDto createUpdatePositionDto, CancellationToken ct = default);

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
    /// <param name="positionQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<PositionVo>> GetListAsync(PositionQueryCriteria positionQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="positionQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(PositionQueryCriteria positionQueryCriteria, CancellationToken ct = default);

    #endregion

    #region 扩展接口

    /// <summary>
    /// 获取所有岗位
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<PositionVo>> GetAllAsync(CancellationToken ct = default);

    #endregion
}
