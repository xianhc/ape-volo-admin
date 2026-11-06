namespace Ape.Volo.Application.Contracts.Abstractions.Logging;

/// <summary>
/// 日志系统自身失败时使用的独立输出，不重新进入数据库日志管道。
/// </summary>
public interface IFallbackLogWriter
{
    /// <summary>
    /// 将诊断信息写入独立兜底输出。
    /// </summary>
    /// <param name="message">诊断信息</param>
    /// <param name="ct">取消令牌</param>
    Task WriteAsync(string message, CancellationToken ct = default);
}
