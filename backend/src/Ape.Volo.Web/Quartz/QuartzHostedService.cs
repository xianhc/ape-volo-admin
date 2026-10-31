namespace Ape.Volo.Web.Quartz;

/// <summary>
/// 将作业装载和调度器关闭接入宿主生命周期。
/// </summary>
/// <param name="provider">根服务提供者</param>
/// <param name="logger">日志记录器</param>
public sealed class QuartzHostedService(IServiceProvider provider, ILogger<QuartzHostedService> logger) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try { await provider.StartQuartzNetJobsAsync(cancellationToken); }
        catch
        {
            // 装载部分成功后失败时，宿主可能尚未进入正常停止流程，立即清理已启动的调度器。
            try { await provider.GetRequiredService<ISchedulerCenterService>().ShutdownScheduleAsync(CancellationToken.None); }
            catch (Exception exception) { logger.LogError(exception, "清理启动失败的调度器时发生异常。"); }
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // 自动装载关闭时仍可能通过管理接口手动启用作业，因此停止阶段也检查调度器。
        if (!await provider.GetRequiredService<ISchedulerCenterService>().ShutdownScheduleAsync(cancellationToken))
            throw new InvalidOperationException("关闭作业调度器失败。");
    }
}
