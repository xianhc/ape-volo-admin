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
/// 字典接口
/// </summary>
public interface IDictService : IBaseServices<Dict>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateDictDto createUpdateDictDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateDictDto createUpdateDictDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="dictQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<DictVo>> QueryAsync(DictQueryCriteria dictQueryCriteria, Pagination pagination);


    /// <summary>
    /// 根据名称查询字典
    /// </summary>
    /// <returns></returns>
    Task<DictVo> QueryByNameAsync(string name);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="dictQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(DictQueryCriteria dictQueryCriteria);

    #endregion
}
