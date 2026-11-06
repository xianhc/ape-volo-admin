using System.Globalization;
using System.Text;
using Ape.Volo.Application.Contracts.Abstractions.Logging;
using Microsoft.Extensions.Hosting;

namespace Ape.Volo.Core.Serilog;

/// <summary>
/// 每个宿主独立的文件兜底输出，不依赖 ILogger，避免日志失败递归。
/// </summary>
/// <param name="environment">宿主环境，决定日志目录</param>
/// <param name="clock">时间提供者</param>
public sealed class FallbackLogWriter(IHostEnvironment environment, TimeProvider clock) : IFallbackLogWriter, IDisposable
{
    private readonly string _directory = Path.Combine(environment.ContentRootPath, "logs");
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task WriteAsync(string message, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var now = clock.GetLocalNow();
            var directory = Path.Combine(_directory, now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(directory);
            var file = Path.Combine(directory, "Fallback.log");
            var expired = Path.Combine(_directory, now.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), "Fallback.log");
            if (File.Exists(expired))
                File.Delete(expired);
            var text = now.ToString("yyyy-MM-dd HH:mm:ss fff", CultureInfo.InvariantCulture) + Environment.NewLine +
                       message + Environment.NewLine + Environment.NewLine;
            await File.AppendAllTextAsync(file, text, Encoding.UTF8, ct);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // 文件系统失败时最后输出到标准错误，不重新进入日志系统。
            await Console.Error.WriteLineAsync($"Fallback log write failed ({exception.GetType().Name}).");
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}
