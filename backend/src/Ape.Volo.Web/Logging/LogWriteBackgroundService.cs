namespace Ape.Volo.Web.Logging;

/// <summary>
/// 日志异步写入后台服务基类
/// </summary>
/// <typeparam name="T">日志实体类型</typeparam>
public abstract class LogWriteBackgroundService<T> : BackgroundService where T : class
{
    /// <summary>
    /// 单批最大写入条数
    /// </summary>
    private const int MaxBatchSize = 200;

    private readonly ILogQueue<T> _logQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _writeCancellation = new();
    private readonly CancellationToken _writeToken;
    private int _disposed;

    /// <summary>
    /// 初始化 <see cref="LogWriteBackgroundService{T}"/> 的新实例。
    /// </summary>
    /// <param name="logQueue">日志队列</param>
    /// <param name="scopeFactory">作用域工厂</param>
    /// <param name="logger">日志记录器</param>
    protected LogWriteBackgroundService(ILogQueue<T> logQueue, IServiceScopeFactory scopeFactory, ILogger logger)
    {
        _logQueue = logQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _writeToken = _writeCancellation.Token;
    }

    /// <summary>
    /// 批量写入日志
    /// </summary>
    /// <param name="serviceProvider">当前作用域的服务提供器</param>
    /// <param name="batch">日志批次</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract Task WriteAsync(IServiceProvider serviceProvider, List<T> batch,
        CancellationToken cancellationToken);

    /// <summary>
    /// 后台循环：按批从队列取日志并落库，停止前写完剩余数据
    /// </summary>
    /// <param name="stoppingToken">服务停止令牌</param>
    /// <returns>异步任务</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var writeToken = _writeToken;
        var batch = new List<T>(MaxBatchSize);
        try
        {
            while (await _logQueue.WaitToReadAsync(stoppingToken).ConfigureAwait(false))
            {
                while (batch.Count < MaxBatchSize && _logQueue.TryDequeue(out var item))
                {
                    batch.Add(item);
                }

                if (batch.Count == 0)
                {
                    continue;
                }

                // 正常停止仅停止等待新数据；已取出的批次继续写完，超出停机期限才取消写入。
                await FlushAsync(batch, writeToken).ConfigureAwait(false);
                batch.Clear();
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested || writeToken.IsCancellationRequested)
        {
            //应用停止，忽略
        }
        finally
        {
            //停止前把队列中剩余的日志写完
            try
            {
                await DrainRemainingAsync(writeToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (writeToken.IsCancellationRequested)
            {
                // 停机期限已到，不再启动新的批次。
            }
        }
    }

    /// <summary>
    /// 正常停止允许批次完成；宿主取消停机等待时才向写入操作传播取消。
    /// </summary>
    /// <param name="cancellationToken">宿主停机取消令牌</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        using var registration = cancellationToken.Register(() => _writeCancellation.Cancel());
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _writeCancellation.Cancel();
        base.Dispose();
        _writeCancellation.Dispose();
    }

    /// <summary>
    /// 应用停止时清空队列
    /// </summary>
    private async Task DrainRemainingAsync(CancellationToken cancellationToken)
    {
        var batch = new List<T>(MaxBatchSize);
        while (_logQueue.TryDequeue(out var item))
        {
            cancellationToken.ThrowIfCancellationRequested();
            batch.Add(item);
            if (batch.Count >= MaxBatchSize)
            {
                await FlushAsync(batch, cancellationToken).ConfigureAwait(false);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await FlushAsync(batch, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 写入一批日志，失败只记录错误不抛出，避免后台服务退出
    /// </summary>
    private async Task FlushAsync(List<T> batch, CancellationToken cancellationToken)
    {
        try
        {
            //每次写入使用独立作用域，保证 IUnitOfWork 等 Scoped 服务生命周期正确
            await using var scope = _scopeFactory.CreateAsyncScope();
            await WriteAsync(scope.ServiceProvider, batch, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            try
            {
                _logger.LogError(ex, "异步写入 {LogType} 日志失败，本批 {Count} 条", typeof(T).Name, batch.Count);
            }
            catch (Exception)
            {
                // 日志允许丢失，错误日志输出失败也不能使后台服务退出并影响宿主运行。
            }
        }
    }
}
