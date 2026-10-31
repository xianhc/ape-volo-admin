using System.Globalization;
using System.Reflection;
using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Exceptions;
using Quartz;
using Quartz.Spi;
using TriggerState = Ape.Volo.Shared.Enums.TriggerState;
using TriggerType = Ape.Volo.Shared.Enums.TriggerType;

namespace Ape.Volo.Web.Quartz;

/// <summary>
/// 宿主独立、延迟创建的 Quartz 适配器，以数据库 ID 幂等替换调度定义。
/// </summary>
public sealed class SchedulerCenterService : ISchedulerCenterService, IAsyncDisposable
{
    internal const string JobIdKey = "ScheduledJobId";
    private const string SchedulerGroup = "ApeVolo.ScheduledJobs";
    private readonly Lazy<Task<IScheduler>> _scheduler;
    private readonly ILogger<SchedulerCenterService> _logger;
    private readonly SemaphoreSlim _mutation = new(1, 1);
    private bool _stopping;
    private int _disposed;

    /// <summary>
    /// 构造时只保存依赖，不启动异步初始化。
    /// </summary>
    /// <param name="factory">Quartz 调度器工厂</param>
    /// <param name="jobFactory">作业实例工厂</param>
    /// <param name="logger">日志记录器</param>
    public SchedulerCenterService(ISchedulerFactory factory, IJobFactory jobFactory, ILogger<SchedulerCenterService> logger)
    {
        _logger = logger;
        _scheduler = new Lazy<Task<IScheduler>>(async () =>
        {
            var scheduler = await factory.GetScheduler();
            scheduler.JobFactory = jobFactory;
            return scheduler;
        });
    }

    /// <inheritdoc />
    public void ValidateConfiguration(ScheduledJobVo job)
    {
        ResolveJobType(job);
        if (job.TriggerType == TriggerType.Cron)
        {
            if (string.IsNullOrWhiteSpace(job.Cron) || !CronExpression.IsValidExpression(job.Cron))
                throw new BadRequestException("cron 格式错误。");
        }
        else if (job.TriggerType != TriggerType.Simple || job.IntervalSecond is <= 0)
            throw new BadRequestException("作业触发器类型或执行间隔无效。");
        if (job.EndTime.HasValue && job.EndTime <= (job.StartTime ?? DateTime.Now))
            throw new BadRequestException("作业结束时间必须晚于开始时间。");
    }

    /// <inheritdoc />
    public Task<bool> StartScheduleAsync(CancellationToken ct = default) => MutateAsync(async scheduler =>
    {
        if (!scheduler.IsStarted) await scheduler.Start(ct);
        return true;
    }, ct);

    /// <inheritdoc />
    public async Task<bool> ShutdownScheduleAsync(CancellationToken ct = default)
    {
        // 未使用的宿主不因停机而初始化调度器。
        await _mutation.WaitAsync(ct);
        IScheduler scheduler;
        try
        {
            _stopping = true;
            if (!_scheduler.IsValueCreated) return true;
            // 初始化属于宿主生命周期，必须取得其最终实例并关闭，不能仅取消等待留下孤立实例。
            scheduler = await _scheduler.Value;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception exception) { _logger.LogError(exception, "关闭作业调度器失败。"); return false; }
        finally { _mutation.Release(); }
        // 等待作业结束时不能持有变更锁，作业自身的失败处理可能调用暂停接口。
        try
        {
            if (!scheduler.IsShutdown) await scheduler.Shutdown(waitForJobsToComplete: true, cancellationToken: ct);
            return true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception exception) { _logger.LogError(exception, "关闭作业调度器失败。"); return false; }
    }

    /// <inheritdoc />
    public Task<bool> AddScheduleJobAsync(ScheduledJobVo job, CancellationToken ct = default) => MutateAsync(async scheduler =>
    {
        ValidateConfiguration(job);
        if (job.Id <= 0) throw new BadRequestException("调度作业必须先保存并取得 ID。");
        var key = Key(job.Id);
        var definition = JobBuilder.Create(ResolveJobType(job)).WithIdentity(key).WithDescription(job.Description)
            .UsingJobData(JobIdKey, job.Id).UsingJobData("JobParam", job.RunParams ?? string.Empty).Build();
        var builder = TriggerBuilder.Create().WithIdentity(TriggerKey(job.Id)).ForJob(key)
            .StartAt(DateBuilder.NextGivenSecondDate(job.StartTime ?? DateTime.Now, 1));
        if (job.EndTime.HasValue) builder.EndAt(new DateTimeOffset(job.EndTime.Value));
        if (job.TriggerType == TriggerType.Cron)
            builder.WithCronSchedule(job.Cron!, schedule => schedule.WithMisfireHandlingInstructionDoNothing());
        else
            builder.WithSimpleSchedule(schedule =>
            {
                schedule.WithIntervalInSeconds(job.IntervalSecond ?? 10);
                if (job.CycleRunTimes > 0) schedule.WithRepeatCount(job.CycleRunTimes.Value - 1);
                else schedule.RepeatForever();
            });

        // Quartz 在同一次替换中更新任务和触发器；改名不改变身份，不先删除再创建。
        await scheduler.ScheduleJob(definition, new HashSet<ITrigger> { builder.Build() }, replace: true, cancellationToken: ct);
        if (!scheduler.IsStarted) await scheduler.Start(ct);
        return true;
    }, ct);

    /// <inheritdoc />
    public Task<bool> DeleteScheduleJobAsync(long id, CancellationToken ct = default) => MutateAsync(async scheduler =>
    {
        await scheduler.DeleteJob(Key(id), ct);
        return true; // 已不存在时同样达成“停止调度”的目标。
    }, ct);

    /// <inheritdoc />
    public Task<bool> PauseJob(long id, CancellationToken ct = default) => MutateAsync(async scheduler =>
    {
        if (!await scheduler.CheckExists(Key(id), ct)) return false;
        await scheduler.PauseJob(Key(id), ct);
        return true;
    }, ct);

    /// <inheritdoc />
    public Task<bool> ResumeJob(long id, CancellationToken ct = default) => MutateAsync(async scheduler =>
    {
        if (!await scheduler.CheckExists(Key(id), ct)) return false;
        await scheduler.ResumeJob(Key(id), ct);
        return true;
    }, ct);

    /// <inheritdoc />
    public async Task<bool> IsExistScheduleJobAsync(long id, CancellationToken ct = default)
        => await (await _scheduler.Value.WaitAsync(ct)).CheckExists(Key(id), ct);

    /// <inheritdoc />
    public async Task<TriggerState> GetTriggerStatus(long id, CancellationToken ct = default)
        => (TriggerState)(int)await (await _scheduler.Value.WaitAsync(ct)).GetTriggerState(TriggerKey(id), ct);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        await ShutdownScheduleAsync();
        _mutation.Dispose();
    }

    private async Task<bool> MutateAsync(Func<IScheduler, Task<bool>> action, CancellationToken ct)
    {
        await _mutation.WaitAsync(ct);
        try
        {
            if (_stopping) return false;
            return await action(await _scheduler.Value.WaitAsync(ct));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception exception) { _logger.LogError(exception, "同步作业调度失败。"); return false; }
        finally { _mutation.Release(); }
    }

    private static JobKey Key(long id) => new(id.ToString(CultureInfo.InvariantCulture), SchedulerGroup);
    private static TriggerKey TriggerKey(long id) => new(id.ToString(CultureInfo.InvariantCulture), SchedulerGroup);

    private static Type ResolveJobType(ScheduledJobVo job)
    {
        try
        {
            var assembly = Assembly.Load(new AssemblyName(job.AssemblyName));
            var type = assembly.GetType(job.ClassName) ?? assembly.GetType(job.AssemblyName + "." + job.ClassName)
                       ?? assembly.DefinedTypes.FirstOrDefault(type => type.Name == job.ClassName)?.AsType();
            if (type is { IsAbstract: false } && typeof(IJob).IsAssignableFrom(type)) return type;
        }
        catch (Exception exception) when (exception is ArgumentException or FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            throw new BadRequestException("作业程序集或执行类型无效。");
        }
        throw new BadRequestException("作业执行类型必须实现 Quartz.IJob。");
    }
}
