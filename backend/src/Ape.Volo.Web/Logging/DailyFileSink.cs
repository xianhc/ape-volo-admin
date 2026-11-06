using System.Globalization;
using System.Text;
using Ape.Volo.Core.Serilog;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 按日志发生日期切换文件，保持每类日志最多 31 个日期文件。
/// </summary>
internal sealed class DailyFileSink(string contentRootPath, string fileName) : ILogEventSink, IDisposable
{
    private readonly string _root = Path.Combine(contentRootPath, "logs");
    private readonly object _gate = new();
    private Logger? _logger;
    private string? _date;

    public void Emit(LogEvent logEvent)
    {
        lock (_gate)
        {
            var date = logEvent.Timestamp.LocalDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (_date != date)
            {
                _logger?.Dispose();
                _logger = null;
                _date = null;
                _logger = new LoggerConfiguration().MinimumLevel.Verbose()
                    .WriteTo.File(Path.Combine(_root, date, fileName),
                        outputTemplate: LoggerProperty.MessageTemplate, encoding: Encoding.UTF8)
                    .CreateLogger();
                _date = date;
                RemoveExpiredFiles(date);
            }

            _logger!.Write(logEvent);
        }
    }

    private void RemoveExpiredFiles(string currentDate)
    {
        try
        {
            var expired = Directory.EnumerateDirectories(_root)
                .Where(path => DateOnly.TryParseExact(Path.GetFileName(path), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                .Where(path => Path.GetFileName(path) != currentDate && File.Exists(Path.Combine(path, fileName)))
                .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
                .Skip(30);
            foreach (var directory in expired)
            {
                File.Delete(Path.Combine(directory, fileName));
                if (!Directory.EnumerateFileSystemEntries(directory).Any())
                    Directory.Delete(directory);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            SelfLog.WriteLine("Failed to remove expired log files: {0}", exception);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _logger?.Dispose();
            _logger = null;
            _date = null;
        }
    }
}
