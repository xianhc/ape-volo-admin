using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Log;

/// <summary>
/// ScheduledJob日志接口
/// </summary>
public interface IScheduledJobLogService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="scheduledJobLogDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    // 执行日志由作业宿主写入，HTTP 端只提供查询和导出。
    [RemoteService(false)]
    Task<OperateResult> CreateAsync(ScheduledJobLogDto scheduledJobLogDto, CancellationToken ct = default);

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
