using Quartz;
using Quartz.Spi;

namespace Ape.Volo.Web.Quartz;

/// <summary>
/// 执行时创建异步作用域，Quartz 的同步归还接口不承担异步资源释放。
/// </summary>
/// <param name="scopeFactory">服务作用域工厂</param>
public sealed class JobFactory(IServiceScopeFactory scopeFactory) : IJobFactory
{
    /// <inheritdoc />
    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        var type = bundle.JobDetail.JobType;
        if (!typeof(IJob).IsAssignableFrom(type) || type.IsAbstract)
            throw new InvalidOperationException($"作业类型 {type.FullName} 未实现可实例化的 Quartz.IJob。");
        return new ScopedJob(scopeFactory, type);
    }

    /// <inheritdoc />
    public void ReturnJob(IJob job)
    {
        // 作用域由 ScopedJob.Execute 的 await using 完整释放，未执行的包装器没有作用域。
    }

    private sealed class ScopedJob(IServiceScopeFactory factory, Type type) : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            await using var scope = factory.CreateAsyncScope();
            var job = (IJob)scope.ServiceProvider.GetRequiredService(type);
            await job.Execute(context);
        }
    }
}
