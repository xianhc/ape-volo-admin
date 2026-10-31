using System.Threading.Channels;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 基于 Channel 的日志异步写入队列
/// </summary>
/// <typeparam name="T">日志实体类型</typeparam>
public sealed class LogQueue<T> : ILogQueue<T> where T : class
{
    /// <summary>
    /// 默认队列容量
    /// </summary>
    public const int DefaultCapacity = 10000;

    private readonly Channel<T> _channel;
    private long _droppedCount;

    /// <summary>
    /// 初始化 <see cref="LogQueue{T}"/> 的新实例。
    /// </summary>
    public LogQueue()
    {
        //有界队列：容量满时 TryWrite 返回 false，由调用方统计丢弃，避免无限增长导致内存溢出
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(DefaultCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    }

    /// <summary>
    /// 累计被丢弃的日志条数（队列满时产生）
    /// </summary>
    public long DroppedCount => Interlocked.Read(ref _droppedCount);

    /// <summary>
    /// 尝试把日志写入队列，队列已满时不阻塞而是计入丢弃数
    /// </summary>
    /// <param name="item">日志对象</param>
    /// <returns>写入成功返回 true</returns>
    public bool TryEnqueue(T item)
    {
        if (item == null)
        {
            return false;
        }

        if (_channel.Writer.TryWrite(item))
        {
            return true;
        }

        Interlocked.Increment(ref _droppedCount);
        return false;
    }

    /// <summary>
    /// 尝试从队列取出一条日志
    /// </summary>
    /// <param name="item">取出的日志对象</param>
    /// <returns>取到数据返回 true</returns>
    public bool TryDequeue(out T item)
    {
        return _channel.Reader.TryRead(out item!);
    }

    /// <summary>
    /// 等待队列中产生可读数据，供后台服务循环消费
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>队列可读时为 true，队列已完成写入时为 false</returns>
    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.WaitToReadAsync(cancellationToken);
    }
}
