using System.Reflection;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Application.Contracts.Abstractions;
using Ape.Volo.Application.Contracts.Abstractions.Logging;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Entity.Attributes;
using Ape.Volo.Entity.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Events;
using Serilog.Core;

namespace Ape.Volo.Core.Serilog;

/// <summary>
/// 把 Serilog 日志按等级写入对应分表
/// </summary>
public class LoggerToDbSink : IBatchedLogEventSink
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LogDataConnectionOptions _logDataConnectionOptions;
    private readonly IFallbackLogWriter _fallbackLogWriter;
    private readonly IIdGenerator _idGenerator;

    /// <summary>
    /// 初始化 <see cref="LoggerToDbSink"/> 的新实例。
    /// </summary>
    /// <param name="scopeFactory">作用域工厂。Sink 是单例，不能直接持有 scoped 的 IUnitOfWork</param>
    /// <param name="logDataConnectionOptions">日志库连接配置</param>
    /// <param name="fallbackLogWriter">独立兜底输出</param>
    /// <param name="idGenerator">唯一 ID 生成器</param>
    public LoggerToDbSink(IServiceScopeFactory scopeFactory,
        IOptions<LogDataConnectionOptions> logDataConnectionOptions, IFallbackLogWriter fallbackLogWriter,
        IIdGenerator idGenerator)
    {
        _scopeFactory = scopeFactory;
        _logDataConnectionOptions = logDataConnectionOptions.Value;
        _fallbackLogWriter = fallbackLogWriter;
        _idGenerator = idGenerator;
    }

    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
        try
        {
            // 每批日志独立作用域，避免单例 Sink 长期持有 scoped 服务
            await using var scope = _scopeFactory.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var logEvents = batch.ToList();
            await Record<AopSqlLog>(unitOfWork, logEvents.RecordSql());
            await RecordLog(unitOfWork, logEvents.RecordLog());
        }
        catch (Exception exception)
        {
            await _fallbackLogWriter.WriteAsync(exception.ToString());
        }
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }

    private async Task RecordLog(IUnitOfWork unitOfWork, List<LogEvent> logEvents)
    {
        if (logEvents.Count == 0)
        {
            return;
        }

        foreach (var group in logEvents.GroupBy(s => s.Level))
        {
            switch (group.Key)
            {
                case LogEventLevel.Information:
                    await Record<InformationLog>(unitOfWork, group.ToList());
                    break;
                case LogEventLevel.Warning:
                    await Record<WarningLog>(unitOfWork, group.ToList());
                    break;
                case LogEventLevel.Error:
                    await Record<ErrorLog>(unitOfWork, group.ToList());
                    break;
                case LogEventLevel.Fatal:
                    await Record<FatalLog>(unitOfWork, group.ToList());
                    break;
            }
        }
    }

    /// <summary>
    /// 将一批日志事件写入 TLog 对应的分表
    /// </summary>
    private async Task Record<TLog>(IUnitOfWork unitOfWork, List<LogEvent> logEvents)
        where TLog : LogEntityBase, new()
    {
        if (logEvents.Count == 0)
        {
            return;
        }

        var logs = logEvents.Select(logEvent => new TLog
        {
            Id = _idGenerator.NewId(),
            CreationTime = logEvent.Timestamp.DateTime,
            Level = logEvent.Level.ToString(),
            Message = logEvent.RenderMessage(),
            MessageTemplate = logEvent.MessageTemplate.Text,
            Properties = logEvent.Properties.ToJson()
        }).ToList();

        try
        {
            //只有标记了 LogDataBase 的实体才写日志库
            if (typeof(TLog).GetCustomAttribute<LogDataBaseAttribute>() == null)
            {
                return;
            }

            var sugarClient = unitOfWork.GetDbClient()
                .GetConnectionScope(_logDataConnectionOptions.ConnId);
            await sugarClient.Insertable(logs).SplitTable().ExecuteCommandAsync();
        }
        catch (Exception e)
        {
            await _fallbackLogWriter.WriteAsync(e.ToString());
            ConsoleHelper.WriteLine(e.ToString(), ConsoleColor.Red);
        }
    }
}
