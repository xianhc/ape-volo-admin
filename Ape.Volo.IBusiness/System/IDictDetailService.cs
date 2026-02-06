using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.System.Dict;
using Ape.Volo.SharedModel.Dto.Core.System.Dict;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System.Dict;

namespace Ape.Volo.IBusiness.System;

/// <summary>
/// 字典详情接口
/// </summary>
public interface IDictDetailService : IBaseServices<DictDetail>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDictDetailDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDictDetailDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateDictDetailDto createUpdateDictDetailDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);


    /// <summary>
    /// 查询 根据字典ID
    /// </summary>
    /// <param name="dictId"></param>
    /// <returns></returns>
    Task<List<DictDetailVo>> GetDetailByDictIdAsync(long dictId);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="dictDetailQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<DictDetailVo>> QueryAsync(DictDetailQueryCriteria dictDetailQueryCriteria, Pagination pagination);

    #endregion
}
