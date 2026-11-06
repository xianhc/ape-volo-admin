using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.DependencyInjection;
using Ape.Volo.Application.Contracts.Abstractions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Configuration;
using Ape.Volo.Application.Contracts.Abstractions;

namespace Ape.Volo.Core.Serilog;

/// <summary>
/// 数据库日志 Sink 的装配扩展
/// </summary>
public static class LoggerToDbSinkConfiguration
{
    /// <summary>
    /// 把日志写入数据库
    /// </summary>
    /// <param name="loggerConfiguration">Serilog 配置</param>
    /// <param name="scopeFactory">作用域工厂</param>
    /// <param name="logDataConnectionOptions">日志库连接配置</param>
    /// <param name="fallbackLogWriter">独立兜底输出</param>
    /// <param name="idGenerator">唯一 ID 生成器</param>
    /// <returns>Serilog 配置</returns>
    public static LoggerConfiguration WriteToDb(this LoggerConfiguration loggerConfiguration,
        IServiceScopeFactory scopeFactory, LogDataConnectionOptions logDataConnectionOptions,
        IFallbackLogWriter fallbackLogWriter, IIdGenerator idGenerator)
    {
        var loggerToDbSink = new LoggerToDbSink(scopeFactory, Options.Create(logDataConnectionOptions),
            fallbackLogWriter, idGenerator);
        var batchingOptions = new BatchingOptions
        {
            BatchSizeLimit = 500,
            BufferingTimeLimit = TimeSpan.FromSeconds(1),
            EagerlyEmitFirstEvent = true,
            QueueLimit = 10000
        };

        return loggerConfiguration.WriteTo.Sink(loggerToDbSink, batchingOptions);
    }
}
