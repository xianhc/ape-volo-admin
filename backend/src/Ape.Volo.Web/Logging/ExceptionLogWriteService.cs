using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Log;
using SqlSugar;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 异常日志异步写入服务
/// </summary>
public sealed class ExceptionLogWriteService : LogWriteBackgroundService<ExceptionLog>
{
    /// <summary>
    /// 初始化 <see cref="ExceptionLogWriteService"/> 的新实例。
    /// </summary>
    /// <param name="logQueue">logQueue</param>
    /// <param name="scopeFactory">scopeFactory</param>
    /// <param name="logger">logger</param>

    public ExceptionLogWriteService(ILogQueue<ExceptionLog> logQueue, IServiceScopeFactory scopeFactory,
        ILogger<ExceptionLogWriteService> logger)
        : base(logQueue, scopeFactory, logger)
    {
    }

    /// <summary>
    /// 批量写入异常日志
    /// </summary>
    /// <param name="serviceProvider">当前作用域的服务提供器</param>
    /// <param name="batch">日志批次</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    protected override async Task WriteAsync(IServiceProvider serviceProvider, List<ExceptionLog> batch,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var repository = serviceProvider.GetRequiredService<ISugarRepository<ExceptionLog>>();
        // 分表插入没有取消重载，等待写入结束后再释放当前作用域。
        var result = await repository.SugarClient.Insertable(batch).SplitTable().ExecuteCommandAsync();
        if (result <= 0) throw new InvalidOperationException("异常日志批量写入未成功。");
    }
}
