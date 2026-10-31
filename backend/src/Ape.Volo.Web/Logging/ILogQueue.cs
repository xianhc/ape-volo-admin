

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 日志异步写入队列
/// </summary>
/// <typeparam name="T">日志实体类型</typeparam>
public interface ILogQueue<T> where T : class
{
    /// <summary>
    /// 入队，队列已满时返回 false（日志被丢弃）
    /// </summary>
    /// <param name="item">日志对象</param>
    /// <returns>是否入队成功</returns>
    bool TryEnqueue(T item);

    /// <summary>
    /// 出队一条，无数据时返回 false
    /// </summary>
    /// <param name="item">日志对象</param>
    /// <returns>是否取到数据</returns>
    bool TryDequeue(out T item);

    /// <summary>
    /// 等待队列中有数据可读
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>队列已完成时为 false</returns>
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 累计被丢弃的日志条数
    /// </summary>
    long DroppedCount { get; }
}
