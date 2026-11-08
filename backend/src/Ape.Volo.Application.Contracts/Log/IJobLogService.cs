using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Log;

/// <summary>
/// Job日志接口
/// </summary>
public interface IJobLogService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="jobLogQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<JobLogVo>> GetListAsync(JobLogQueryCriteria jobLogQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="jobLogQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(JobLogQueryCriteria jobLogQueryCriteria, CancellationToken ct = default);

    #endregion
}
