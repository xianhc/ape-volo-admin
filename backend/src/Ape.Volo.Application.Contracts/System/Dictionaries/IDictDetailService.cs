using Ape.Volo.Application.Contracts.System.Dictionaries.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.Dictionaries;

/// <summary>
/// 字典详情接口
/// </summary>
public interface IDictDetailService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDictDetailDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDictDetailDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto, CancellationToken ct = default);

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
    /// <param name="dictDetailQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<DictDetailVo>> GetListAsync(DictDetailQueryCriteria dictDetailQueryCriteria, CancellationToken ct = default);

    #endregion
}
