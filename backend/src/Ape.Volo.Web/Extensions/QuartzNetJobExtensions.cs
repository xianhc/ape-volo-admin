using Ape.Volo.Web.Quartz;
using Ape.Volo.Common.Extensions;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.ConfigOptions;
using Quartz;
using Quartz.Spi;
using Quartz.Impl;
using System.Collections.Specialized;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// QuartzNet作业启动器
/// </summary>
public static class QuartzNetJobExtensions
{
    /// <summary>
    /// 注册 Quartz 调度中心与作业工厂，并扫描当前程序集自动注入全部作业
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddQuartzNetJobService(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));


        services.AddSingleton<IJobFactory, JobFactory>();
        services.AddSingleton<ISchedulerFactory>(_ => new StdSchedulerFactory(new NameValueCollection
        {
            ["quartz.scheduler.instanceName"] = "ApeVolo-" + Guid.NewGuid().ToString("N"),
            ["quartz.serializer.type"] = "binary"
        }));
        services.AddSingleton<SchedulerCenterService>();
        services.AddSingleton<ISchedulerCenterService>(provider => provider.GetRequiredService<SchedulerCenterService>());
        services.AddSingleton<IJobScheduler>(provider => provider.GetRequiredService<SchedulerCenterService>());
        services.AddHostedService<QuartzHostedService>();
        //任务注入：作业与宿主同程序集，直接扫描 Api 自身，无需按 dll 名加载
        var baseType = typeof(IJob);
        var types = typeof(QuartzNetJobExtensions).Assembly.DefinedTypes
            .Select(type => type.AsType())
            .Where(x => x != baseType && baseType.IsAssignableFrom(x)).ToArray();
        var implementTypes = types.Where(x => x.IsClass && !x.IsAbstract).ToArray();
        foreach (var implementType in implementTypes)
        {
            services.AddTransient(implementType);
        }
    }

    /// <summary>
    /// 启动时把已启用的作业装载进调度器
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <param name="ct">启动取消令牌</param>
    public static Task UseQuartzNetJobMiddlewareAsync(this IApplicationBuilder app, CancellationToken ct = default)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));
        return app.ApplicationServices.StartQuartzNetJobsAsync(ct);
    }

    internal static async Task StartQuartzNetJobsAsync(this IServiceProvider provider, CancellationToken ct)
    {
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(QuartzNetJobExtensions));
        try
        {
            if (provider.GetRequiredService<IOptions<MiddlewareOptions>>().Value.QuartzNetJob)
            {
                ct.ThrowIfCancellationRequested();
                await using var scope = provider.CreateAsyncScope();
                var scheduledJobService = scope.ServiceProvider.GetRequiredService<IScheduledJobStore>();
                var schedulerCenter = scope.ServiceProvider.GetRequiredService<ISchedulerCenterService>();
                var allTaskQuartzList = await scheduledJobService.GetAllAsync(ct);
                ct.ThrowIfCancellationRequested();
                foreach (var item in allTaskQuartzList)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!item.Enabled) continue;
                    var results = await schedulerCenter.AddScheduleJobAsync(item, ct);
                    ct.ThrowIfCancellationRequested();
                    if (results)
                    {
                        logger.LogInformation("作业调度 {JobName} 启动成功。", item.JobName);
                    }
                    else
                    {
                        // 保留管理接口可用，单个配置错误可在启动后修正并重试同步。
                        logger.LogError("作业调度 {JobId} {JobName} 启动失败，请修正配置并重新保存。", item.Id, item.JobName);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "启动作业调度失败。");
            throw;
        }
    }
}
