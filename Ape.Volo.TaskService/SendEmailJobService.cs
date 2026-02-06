using System.Threading.Tasks;
using Ape.Volo.IBusiness.Log;
using Ape.Volo.IBusiness.Message.Email;
using Ape.Volo.IBusiness.System;
using Ape.Volo.TaskService.service;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Ape.Volo.TaskService;

public class SendEmailJobService : JobBase<SendEmailJobService>, IJob
{
    private readonly IEmailScheduleTask _emailScheduleTask;

    public SendEmailJobService(ISchedulerCenterService schedulerCenterService, IQuartzNetService quartzNetService,
        IQuartzNetLogService quartzNetLogService, IEmailScheduleTask emailScheduleTask,
        ILogger<SendEmailJobService> logger)
    {
        QuartzNetService = quartzNetService;
        QuartzNetLogService = quartzNetLogService;
        _emailScheduleTask = emailScheduleTask;
        SchedulerCenterService = schedulerCenterService;
        Logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await ExecuteJob(context, async () => await Run(context));
    }

    private async Task Run(IJobExecutionContext context)
    {
        await _emailScheduleTask.ExecuteAsync();
        //获取传递参数
        //JobDataMap data = context.JobDetail.JobDataMap;
    }
}