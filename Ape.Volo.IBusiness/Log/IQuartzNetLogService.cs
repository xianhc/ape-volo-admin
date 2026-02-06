using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System;

namespace Ape.Volo.IBusiness.Log;

/// <summary>
/// QuartzJob日志接口
/// </summary>
public interface IQuartzNetLogService : IBaseServices<QuartzNetLog>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="quartzNetLog"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(QuartzNetLog quartzNetLog);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="quartzNetLogQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<QuartzNetLogVo>> QueryAsync(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria, Pagination pagination);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="quartzNetLogQueryCriteria"></param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria);

    #endregion
}
