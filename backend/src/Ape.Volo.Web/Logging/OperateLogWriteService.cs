using Ape.Volo.Application.Contracts.Log.Models;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 操作日志异步写入服务
/// </summary>
public sealed class OperateLogWriteService : LogWriteBackgroundService<OperateLogDto>
{
    /// <summary>
    /// 初始化 <see cref="OperateLogWriteService"/> 的新实例。
    /// </summary>
    /// <param name="logQueue">logQueue</param>
    /// <param name="scopeFactory">scopeFactory</param>
    /// <param name="logger">logger</param>

    public OperateLogWriteService(ILogQueue<OperateLogDto> logQueue, IServiceScopeFactory scopeFactory,
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
    protected override async Task WriteAsync(IServiceProvider serviceProvider, List<OperateLogDto> batch,
        CancellationToken cancellationToken)
    {
        var result = await serviceProvider.GetRequiredService<IOperateLogService>().CreateListAsync(batch, cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("操作日志批量写入未成功。");
    }
}
