using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Operations;

/// <summary>
/// 编排配置保存与运行时同步，失败结果区分持久化和调度状态。
/// </summary>
public interface IScheduledJobManager : IApplicationService
{
    /// <summary>
    /// 保存新作业并同步运行时。
    /// </summary>
    /// <param name="request">作业配置</param>
    /// <param name="ct">取消令牌</param>
    Task<OperateResult> CreateAsync(CreateUpdateScheduledJobDto request, CancellationToken ct = default);
    /// <summary>
    /// 保存配置并同步运行时，重复保存同 ID 可重试调度。
    /// </summary>
    /// <param name="request">作业配置</param>
    /// <param name="ct">取消令牌</param>
    Task<OperateResult> UpdateAsync(CreateUpdateScheduledJobDto request, CancellationToken ct = default);
    /// <summary>
    /// 先停止运行时，成功后再删除配置。
    /// </summary>
    /// <param name="ids">待删除的作业 ID 集合</param>
    /// <param name="ct">取消令牌</param>
    Task<OperateResult> DeleteAsync(IEnumerable<long> ids, CancellationToken ct = default);
    /// <summary>
    /// 启用配置并同步运行时。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<OperateResult> ExecuteAsync(long id, CancellationToken ct = default);
    /// <summary>
    /// 暂停当前运行时作业。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<OperateResult> PauseAsync(long id, CancellationToken ct = default);
    /// <summary>
    /// 恢复当前运行时作业。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<OperateResult> ResumeAsync(long id, CancellationToken ct = default);
}
