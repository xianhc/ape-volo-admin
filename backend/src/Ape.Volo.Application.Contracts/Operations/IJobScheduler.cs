using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Operations;

/// <summary>
/// 运行时端口，以数据库 ID 标识作业，不暴露 Quartz 类型。
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// 保存前校验运行时配置，无调度副作用。
    /// </summary>
    /// <param name="job">待校验的作业配置</param>
    void ValidateConfiguration(ScheduledJobVo job);
    /// <summary>
    /// 幂等创建或替换同 ID 作业及其触发器。
    /// </summary>
    /// <param name="job">作业配置</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> AddScheduleJobAsync(ScheduledJobVo job, CancellationToken ct = default);
    /// <summary>
    /// 移除作业，已不存在也视为成功。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> DeleteScheduleJobAsync(long id, CancellationToken ct = default);
    /// <summary>
    /// 暂停作业。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> PauseJob(long id, CancellationToken ct = default);
    /// <summary>
    /// 恢复作业。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> ResumeJob(long id, CancellationToken ct = default);
    /// <summary>
    /// 查询运行时是否存在作业。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> IsExistScheduleJobAsync(long id, CancellationToken ct = default);
    /// <summary>
    /// 查询触发器状态。
    /// </summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<TriggerState> GetTriggerStatus(long id, CancellationToken ct = default);
}
