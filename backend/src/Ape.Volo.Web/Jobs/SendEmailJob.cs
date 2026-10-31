using Ape.Volo.Web.Quartz;
using Quartz;

namespace Ape.Volo.Web.Jobs;

/// <summary>
/// 发送邮件作业：触发应用层邮件队列投递并统一记录作业执行日志
/// </summary>
public class SendEmailJob : JobBase<SendEmailJob>, IJob
{
    private readonly IQueuedEmailService _queuedEmailService;

    /// <summary>
    /// 初始化 <see cref="SendEmailJob"/> 的新实例。
    /// </summary>
    /// <param name="schedulerCenterService">调度中心服务</param>
    /// <param name="scheduledJobService">作业配置服务</param>
    /// <param name="scheduledJobLogService">作业日志服务</param>
    /// <param name="queuedEmailService">邮件队列服务</param>
    /// <param name="logger">日志记录器</param>
    public SendEmailJob(ISchedulerCenterService schedulerCenterService, IScheduledJobStore scheduledJobService,
        IScheduledJobLogService scheduledJobLogService, IQueuedEmailService queuedEmailService,
        ILogger<SendEmailJob> logger)
    {
        ScheduledJobService = scheduledJobService;
        ScheduledJobLogService = scheduledJobLogService;
        _queuedEmailService = queuedEmailService;
        SchedulerCenterService = schedulerCenterService;
        Logger = logger;
    }

    /// <summary>
    /// 作业执行入口，通过基类统一记录执行日志
    /// </summary>
    /// <param name="context">Quartz 作业执行上下文</param>
    /// <returns>异步任务</returns>
    public async Task Execute(IJobExecutionContext context)
    {
        await ExecuteJob(context, async () => await Run(context));
    }

    private async Task Run(IJobExecutionContext context)
    {
        await _queuedEmailService.SendPendingAsync(context.CancellationToken);
    }
}
