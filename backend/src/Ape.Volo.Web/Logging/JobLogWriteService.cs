using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Log;
using SqlSugar;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 作业日志异步写入服务
/// </summary>
public sealed class JobLogWriteService : LogWriteBackgroundService<JobLog>
{
    /// <summary>
    /// 初始化作业日志后台写入服务。
    /// </summary>
    /// <param name="logQueue">作业日志队列</param>
    /// <param name="scopeFactory">作用域工厂</param>
    /// <param name="logger">日志记录器</param>
    public JobLogWriteService(ILogQueue<JobLog> logQueue, IServiceScopeFactory scopeFactory,
        ILogger<JobLogWriteService> logger) : base(logQueue, scopeFactory, logger)
    {
    }

    /// <summary>
    /// 批量写入作业日志月分表。
    /// </summary>
    /// <param name="serviceProvider">当前作用域的服务提供器</param>
    /// <param name="batch">日志批次</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    protected override async Task WriteAsync(IServiceProvider serviceProvider, List<JobLog> batch,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var repository = serviceProvider.GetRequiredService<ISugarRepository<JobLog>>();
        // 分表插入没有取消重载，等待写入结束后再释放当前作用域。
        var result = await repository.SugarClient.Insertable(batch).SplitTable().ExecuteCommandAsync();
        if (result <= 0) throw new InvalidOperationException("作业日志批量写入未成功。");
    }
}
