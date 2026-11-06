using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Log;
using SqlSugar;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 操作日志异步写入服务
/// </summary>
public sealed class OperateLogWriteService : LogWriteBackgroundService<OperateLog>
{
    /// <summary>
    /// 初始化 <see cref="OperateLogWriteService"/> 的新实例。
    /// </summary>
    /// <param name="logQueue">logQueue</param>
    /// <param name="scopeFactory">scopeFactory</param>
    /// <param name="logger">logger</param>

    public OperateLogWriteService(ILogQueue<OperateLog> logQueue, IServiceScopeFactory scopeFactory,
        ILogger<OperateLogWriteService> logger)
        : base(logQueue, scopeFactory, logger)
    {
    }

    /// <summary>
    /// 批量写入操作日志
    /// </summary>
    /// <param name="serviceProvider">当前作用域的服务提供器</param>
    /// <param name="batch">日志批次</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    protected override async Task WriteAsync(IServiceProvider serviceProvider, List<OperateLog> batch,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var repository = serviceProvider.GetRequiredService<ISugarRepository<OperateLog>>();
        // 分表插入没有取消重载，等待写入结束后再释放当前作用域。
        var result = await repository.SugarClient.Insertable(batch).SplitTable().ExecuteCommandAsync();
        if (result <= 0) throw new InvalidOperationException("操作日志批量写入未成功。");
    }
}
