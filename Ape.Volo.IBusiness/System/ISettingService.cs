using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System;

namespace Ape.Volo.IBusiness.System;

/// <summary>
/// 全局设置接口
/// </summary>
public interface ISettingService : IBaseServices<Setting>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(CreateUpdateSettingDto createUpdateSettingDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateSettingDto createUpdateSettingDto);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="parameterQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<SettingVo>> QueryAsync(ParameterQueryCriteria parameterQueryCriteria, Pagination pagination);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="parameterQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(ParameterQueryCriteria parameterQueryCriteria);

    /// <summary>
    /// 根据名称查询
    /// </summary>
    /// <param name="settingName"></param>
    /// <returns></returns>
    Task<T> GetSettingValue<T>(string settingName);

    #endregion
}