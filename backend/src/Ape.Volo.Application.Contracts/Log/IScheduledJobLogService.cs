using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Log;

/// <summary>
/// ScheduledJob日志接口
/// </summary>
public interface IScheduledJobLogService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="scheduledJobLogQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<ScheduledJobLogVo>> GetListAsync(ScheduledJobLogQueryCriteria scheduledJobLogQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="scheduledJobLogQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(ScheduledJobLogQueryCriteria scheduledJobLogQueryCriteria, CancellationToken ct = default);

    #endregion
}
