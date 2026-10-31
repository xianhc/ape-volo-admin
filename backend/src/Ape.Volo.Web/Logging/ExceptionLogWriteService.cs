using Ape.Volo.Application.Contracts.Log.Models;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 异常日志异步写入服务
/// </summary>
public sealed class ExceptionLogWriteService : LogWriteBackgroundService<ExceptionLogDto>
{
    /// <summary>
    /// 初始化 <see cref="ExceptionLogWriteService"/> 的新实例。
    /// </summary>
    /// <param name="logQueue">logQueue</param>
    /// <param name="scopeFactory">scopeFactory</param>
    /// <param name="logger">logger</param>

    public ExceptionLogWriteService(ILogQueue<ExceptionLogDto> logQueue, IServiceScopeFactory scopeFactory,
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
    protected override async Task WriteAsync(IServiceProvider serviceProvider, List<ExceptionLogDto> batch,
        CancellationToken cancellationToken)
    {
        var result = await serviceProvider.GetRequiredService<IExceptionLogService>().CreateListAsync(batch, cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("异常日志批量写入未成功。");
    }
}
