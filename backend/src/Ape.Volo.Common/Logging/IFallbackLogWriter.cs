namespace Ape.Volo.Common.Logging;

/// <summary>
/// 日志系统自身失败时使用的独立输出，不重新进入数据库日志管道。
/// </summary>
public interface IFallbackLogWriter
{
    Task WriteAsync(string message, CancellationToken ct = default);
}
