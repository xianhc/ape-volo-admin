using Ape.Volo.Application.Contracts.System.Dictionaries.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.Dictionaries;

/// <summary>
/// 字典接口
/// </summary>
public interface IDictService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateDictDto createUpdateDictDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateDictDto createUpdateDictDto, CancellationToken ct = default);

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
    /// <param name="dictQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<DictVo>> GetListAsync(DictQueryCriteria dictQueryCriteria, CancellationToken ct = default);


    /// <summary>
    /// 根据名称查询字典
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns>字典，不存在时返回 null</returns>
    Task<DictVo?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="dictQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(DictQueryCriteria dictQueryCriteria, CancellationToken ct = default);

    #endregion
}
