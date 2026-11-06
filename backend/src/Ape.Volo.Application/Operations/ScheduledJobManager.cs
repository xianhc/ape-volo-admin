using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Model;
using Ape.Volo.Shared.Exceptions;

namespace Ape.Volo.Application.Operations;

/// <summary>
/// 数据库保存期望配置，运行时按稳定 ID 幂等同步；两者不是同一个事务。
/// </summary>
/// <param name="jobs">作业持久化服务</param>
/// <param name="scheduler">运行时调度端口</param>
/// <param name="mapper">对象映射器</param>
public sealed class ScheduledJobManager(IScheduledJobStore jobs, IJobScheduler scheduler, IMapper mapper)
    : IScheduledJobManager
{
    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateScheduledJobDto request, CancellationToken ct = default)
    {
        // 保留新增接口原有约束：Simple 作业间隔必须大于 5 秒。
        if (request.TriggerType == TriggerType.Simple && request.IntervalSecond is <= 5)
            throw new BadRequestException("simple模式下请设置作业间隔执行秒数！");
        if (request.Enabled) scheduler.ValidateConfiguration(mapper.MapTo<ScheduledJobVo>(request));
        return await ApplyAsync(await jobs.CreateAsync(request, ct), ct);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateScheduledJobDto request, CancellationToken ct = default)
    {
        if (request.Enabled) scheduler.ValidateConfiguration(mapper.MapTo<ScheduledJobVo>(request));
        var result = await jobs.UpdateAsync(request, ct);
        if (!result.IsSuccess) return result;
        return await ApplyAsync(await jobs.GetByIdAsync(request.Id, ct), ct);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.Delete)]
    public async Task<OperateResult> DeleteAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        var existing = await jobs.GetByIdsAsync(ids, ct);
        if (existing.Count == 0) return OperateResult.Error("作业不存在。");
        foreach (var job in existing)
            if (!await scheduler.DeleteScheduleJobAsync(job.Id, ct))
                return OperateResult.Error("停止调度失败，作业配置已保留，请重试删除。");
        var result = await jobs.DeleteAsync(ids.ToHashSet(), ct);
        return result.IsSuccess ? result : OperateResult.Error("调度已停止，但删除配置失败，请重试删除。");
    }

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<OperateResult> ExecuteAsync(long id, CancellationToken ct = default)
    {
        var job = await jobs.GetByIdAsync(id, ct);
        if (await scheduler.IsExistScheduleJobAsync(id, ct)) return OperateResult.Error("已在运行，请勿重复开启！");
        scheduler.ValidateConfiguration(job);
        job.Enabled = true;
        var result = await jobs.UpdateJobInfoAsync(job, ct);
        return result.IsSuccess ? await ApplyAsync(job, ct) : result;
    }

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<OperateResult> PauseAsync(long id, CancellationToken ct = default)
    {
        await jobs.GetByIdAsync(id, ct);
        if (await scheduler.GetTriggerStatus(id, ct) == TriggerState.Normal && await scheduler.PauseJob(id, ct))
            return OperateResult.Success();
        return OperateResult.Error("暂停失败，请重试！");
    }

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<OperateResult> ResumeAsync(long id, CancellationToken ct = default)
    {
        await jobs.GetByIdAsync(id, ct);
        if (await scheduler.GetTriggerStatus(id, ct) == TriggerState.Paused && await scheduler.ResumeJob(id, ct))
            return OperateResult.Success();
        return OperateResult.Error("恢复失败，请重试！");
    }

    private async Task<OperateResult> ApplyAsync(ScheduledJobVo job, CancellationToken ct)
    {
        var applied = job.Enabled ? await scheduler.AddScheduleJobAsync(job, ct)
            : await scheduler.DeleteScheduleJobAsync(job.Id, ct);
        if (applied) return OperateResult.Success();
        // 替换失败时尽力停止旧配置；重试以同一 ID 同步，不遗留改名前的任务。
        if (job.Enabled) await scheduler.DeleteScheduleJobAsync(job.Id, ct);
        return OperateResult.Error("作业配置已保存，但调度同步失败，请重新保存或执行。");
    }
}
