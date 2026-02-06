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
/// QuartzJob作业接口
/// </summary>
public interface IQuartzNetService : IBaseServices<QuartzNet>
{
    #region 基础接口

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <returns></returns>
    Task<List<QuartzNet>> QueryAllAsync();

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    Task<QuartzNet> CreateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto);

    /// <summary>
    /// 更新任务与日志
    /// </summary>
    /// <param name="quartzNet"></param>
    /// <returns></returns>
    Task<OperateResult> UpdateJobInfoAsync(QuartzNet quartzNet);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="quartzNets"></param>
    /// <returns></returns>
    Task<OperateResult> DeleteAsync(List<QuartzNet> quartzNets);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<QuartzNetVo>> QueryAsync(QuartzNetQueryCriteria quartzNetQueryCriteria, Pagination pagination);

    /// <summary>
    /// 查询全部作业名称
    /// </summary>
    /// <returns></returns>
    Task<List<QuartzNetSmallVo>> QueryAllTaskNameAsync();

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(QuartzNetQueryCriteria quartzNetQueryCriteria);

    #endregion
}