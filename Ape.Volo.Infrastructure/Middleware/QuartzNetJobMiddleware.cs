using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Helper.Serilog;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.IBusiness.System;
using Ape.Volo.TaskService.service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Ape.Volo.Infrastructure.Middleware;

/// <summary>
/// QuartzNet作业调度中间件
/// </summary>
public static class QuartzNetJobMiddleware
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(QuartzNetJobMiddleware));

    public static void UseQuartzNetJobMiddleware(this IApplicationBuilder app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));

        try
        {
            if (App.GetOptions<MiddlewareOptions>().QuartzNetJob)
            {
                var quartzNetService = app.ApplicationServices.GetRequiredService<IQuartzNetService>();
                var schedulerCenter = app.ApplicationServices.GetRequiredService<ISchedulerCenterService>();
                var allTaskQuartzList = AsyncHelper.RunSync(() => quartzNetService.QueryAllAsync());
                foreach (var item in allTaskQuartzList)
                {
                    if (!item.Enabled) continue;
                    var results = AsyncHelper.RunSync(() => schedulerCenter.AddScheduleJobAsync(item));
                    if (results)
                    {
                        Logger.Information(
                            $"{App.L.R("Sys.QuartzNet")}=>{item.TaskName}=>{App.L.R("Action.StartupSuccess")}！");
                    }
                    else
                    {
                        Logger.Error(
                            $"{App.L.R("Sys.QuartzNet")}=>{item.TaskName}=>{App.L.R("Action.StartupFailure")}！");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error starting the job scheduling service:\n{e.Message}");
            throw;
        }
    }
}