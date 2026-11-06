using Ape.Volo.Web.Quartz;
using Ape.Volo.Web.Logging;
using Ape.Volo.Entity.Log;
using Ape.Volo.Application.Contracts.Abstractions.Logging;
using Quartz;

namespace Ape.Volo.Web.Jobs;

/// <summary>
/// 输出当前时间到控制台
/// </summary>
public class TestConsoleWriteJob : JobBase<TestConsoleWriteJob>, IJob
{
    /// <summary>
    /// 初始化 <see cref="TestConsoleWriteJob"/> 的新实例。
    /// </summary>
    /// <param name="schedulerCenterService">调度中心服务</param>
    /// <param name="scheduledJobService">作业配置服务</param>
    /// <param name="scheduledJobLogQueue">作业日志队列</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="idGenerator">唯一 ID 生成器</param>
    /// <param name="fallbackLogWriter">独立文本兜底输出</param>
    public TestConsoleWriteJob(ISchedulerCenterService schedulerCenterService,
        IScheduledJobStore scheduledJobService, ILogQueue<ScheduledJobLog> scheduledJobLogQueue,
        ILogger<TestConsoleWriteJob> logger, IIdGenerator idGenerator, IFallbackLogWriter fallbackLogWriter)
        : base(idGenerator, fallbackLogWriter)
    {
        ScheduledJobService = scheduledJobService;
        ScheduledJobLogQueue = scheduledJobLogQueue;
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
        await Console.Out.WriteLineAsync("当前时间：" + DateTime.Now + "\n");
        //获取传递参数
        JobDataMap data = context.JobDetail.JobDataMap;
    }
}
