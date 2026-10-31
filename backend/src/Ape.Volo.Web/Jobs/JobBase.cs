using System.Diagnostics;
using Ape.Volo.Web.Quartz;
using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.IdGenerator;
using Quartz;

namespace Ape.Volo.Web.Jobs;

/// <summary>
/// 作业调度基类（所有作业须继承）
/// </summary>
public class JobBase<T>
{
    #region 字段

    /// <summary>
    /// 作业配置服务
    /// </summary>
    public IScheduledJobStore ScheduledJobService = default!;

    /// <summary>
    /// 作业日志服务
    /// </summary>
    public IScheduledJobLogService ScheduledJobLogService = default!;

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
        var scheduledJob = await ScheduledJobService.QueryByIdAsync(jobId, ct);
        if (scheduledJob == null)
        {
            await Task.CompletedTask;
            return;
        }

        //JOB组名
        string groupName = scheduledJob.JobGroup;
        //日志
        string jobHistory =
            $"【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】【执行开始】【组别：{groupName} => 任务：{scheduledJob.JobName}";
        //耗时
        try
        {
            stopwatch.Start();
            await func(); //执行任务
            stopwatch.Stop();
            jobHistory += $"，【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】【执行成功】";
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

            jobHistory += $"，【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】【执行失败:{ex.Message}】";
        }
        finally
        {
            var taskSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 3);
            jobHistory += $"，【{DateTime.Now:yyyy-MM-dd HH:mm:ss}】【执行结束】(耗时:{taskSeconds}秒)";
            if (ScheduledJobLogService != null)
            {
                //记录任务日志（作业执行只写日志表，不回写作业主表）
                var scheduledJobLog = new ScheduledJobLogDto
                {
                    Id = IdHelper.NextId(),
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
                await ScheduledJobLogService.CreateAsync(scheduledJobLog, ct);
            }
        }

        Logger.LogInformation(jobHistory);
    }

    #endregion
}
