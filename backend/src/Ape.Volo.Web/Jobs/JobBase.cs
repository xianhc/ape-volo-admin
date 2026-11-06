using System.Diagnostics;
using Ape.Volo.Web.Quartz;
using Ape.Volo.Entity.Log;
using Ape.Volo.Web.Logging;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Application.Contracts.Abstractions;
using Ape.Volo.Application.Contracts.Abstractions.Logging;
using Quartz;

namespace Ape.Volo.Web.Jobs;

/// <summary>
/// 作业调度基类（所有作业须继承）
/// </summary>
public class JobBase<T>
{
    private readonly IIdGenerator _idGenerator;
    private readonly IFallbackLogWriter _fallbackLogWriter;

    /// <summary>
    /// 初始化作业日志所需的唯一 ID 生成器与文本兜底输出。
    /// </summary>
    /// <param name="idGenerator">唯一 ID 生成器</param>
    /// <param name="fallbackLogWriter">独立文本兜底输出</param>
    protected JobBase(IIdGenerator idGenerator, IFallbackLogWriter fallbackLogWriter)
    {
        _idGenerator = idGenerator;
        _fallbackLogWriter = fallbackLogWriter;
    }

    #region 字段

    /// <summary>
    /// 作业配置服务
    /// </summary>
    public IScheduledJobStore ScheduledJobService = default!;

    /// <summary>
    /// 作业日志队列
    /// </summary>
    public ILogQueue<ScheduledJobLog> ScheduledJobLogQueue = default!;

    /// <summary>
    /// 调度中心服务
    /// </summary>
    public ISchedulerCenterService SchedulerCenterService = default!;

    /// <summary>
    /// 日志记录器
    /// </summary>
    public ILogger<T> Logger = default!;

    #endregion

    #region 执行方法

    /// <summary>
    /// 执行指定任务
    /// </summary>
    /// <param name="context"></param>
    /// <param name="func"></param>
    protected async Task ExecuteJob(IJobExecutionContext context, Func<Task> func)
    {
        //是否成功
        bool isSuccess = true;
        //异常详情
        string exceptionDetail = string.Empty;
        //记录Job时间
        Stopwatch stopwatch = new Stopwatch();
        var ct = context.CancellationToken;
        //positionName
        var jobId = context.JobDetail.JobDataMap.GetLong(Ape.Volo.Web.Quartz.SchedulerCenterService.JobIdKey);
        var scheduledJob = await ScheduledJobService.GetByIdAsync(jobId, ct);

        //耗时
        try
        {
            stopwatch.Start();
            await func(); //执行任务
            stopwatch.Stop();
        }
        catch (Exception ex)
        {
            isSuccess = false;
            exceptionDetail = $" {ex.Message}\n{ex.StackTrace}";
            JobExecutionException e2 = new JobExecutionException(ex);
            //失败后是否暂停
            if (scheduledJob.PauseAfterFailure)
            {
                await SchedulerCenterService.PauseJob(scheduledJob.Id, ct);
            }
            else
            {
                //true  是立即重新执行任务 
                e2.RefireImmediately = true;
            }

            //告警邮箱
            if (!scheduledJob.AlertEmail.IsNullOrEmpty())
            {
                //实现自己的告警邮件模板 再添加队列信息
            }
        }
        finally
        {
            try
            {
                // 作业只构建日志并非阻塞入队，数据库写入由后台独立作用域完成。
                var scheduledJobLog = new ScheduledJobLog
                {
                    Id = _idGenerator.NewId(),
                    ScheduledJobId = scheduledJob.Id,
                    JobName = scheduledJob.JobName,
                    JobGroup = scheduledJob.JobGroup,
                    AssemblyName = scheduledJob.AssemblyName,
                    ClassName = scheduledJob.ClassName,
                    Cron = scheduledJob.Cron,
                    ExceptionDetail = exceptionDetail,
                    ExecutionDuration = stopwatch.ElapsedMilliseconds,
                    RunParams = scheduledJob.RunParams,
                    IsSuccess = isSuccess,
                    CreatorName = "ScheduledJob",
                    CreationTime = DateTime.Now
                };
                if (!ScheduledJobLogQueue.TryEnqueue(scheduledJobLog))
                {
                    Logger.LogWarning("作业日志队列已满，本次日志被丢弃（累计丢弃 {Dropped} 条）",
                        ScheduledJobLogQueue.DroppedCount);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await _fallbackLogWriter.WriteAsync(
                        $"作业日志处理失败：{scheduledJob.Id}（{scheduledJob.JobName}）{Environment.NewLine}" +
                        $"执行结果：{(isSuccess ? "成功" : "失败")}，耗时：{stopwatch.ElapsedMilliseconds}ms{Environment.NewLine}" +
                        $"{exceptionDetail}{Environment.NewLine}{ex}",
                        CancellationToken.None);
                }
                catch (Exception)
                {
                    // 兜底输出再次失败时丢弃日志，不能影响作业结果或覆盖原有异常。
                }
            }
        }
    }

    #endregion
}
