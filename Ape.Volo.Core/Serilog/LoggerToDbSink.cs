using System.Reflection;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Entity.Log;
using Ape.Volo.Repository.UnitOfWork;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Ape.Volo.Core.Serilog;

public class LoggerToDbSink : IBatchedLogEventSink
{
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var sugar = App.GetService<IUnitOfWork>();

        var logEvents = batch.ToList();
        await RecordSql(sugar, logEvents.RecordSql());
        await RecordLog(sugar, logEvents.RecordLog());
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

        var group = logEvents.GroupBy(s => s.Level);
        foreach (var v in group)
        {
            switch (v.Key)
            {
                case LogEventLevel.Information:
                    await RecordInformation(unitOfWork, v.ToList());
                    break;
                case LogEventLevel.Warning:
                    await RecordWarning(unitOfWork, v.ToList());
                    break;
                case LogEventLevel.Error:
                    await RecordError(unitOfWork, v.ToList());
                    break;
                case LogEventLevel.Fatal:
                    await RecordFatal(unitOfWork, v.ToList());
                    break;
            }
        }
    }

    private async Task RecordInformation(IUnitOfWork unitOfWork, List<LogEvent> logEvents)
    {
        if (logEvents.Count == 0)
        {
            return;
        }

        var logs = new List<InformationLog>();
        foreach (var logEvent in logEvents)
        {
            var log = new InformationLog
            {
                Id = IdHelper.NextId(),
                CreateTime = logEvent.Timestamp.DateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                MessageTemplate = logEvent.MessageTemplate.Text,
                Properties = logEvent.Properties.ToJson()
            };
            logs.Add(log);
        }

        try
        {
            var sqlSugarScope = unitOfWork.GetDbClient();
            var logDbAttribute = typeof(InformationLog).GetCustomAttribute<LogDataBaseAttribute>();
            if (logDbAttribute != null)
            {
                var sugarClient = sqlSugarScope.GetConnectionScope(App.GetOptions<SystemOptions>().LogDataBase);
                await sugarClient.Insertable(logs).SplitTable().ExecuteCommandAsync();
            }
        }
        catch (Exception e)
        {
            LogHelper.WriteLog(e.ToString(), null);
            ConsoleHelper.WriteLine(e.ToString(), ConsoleColor.Red);
        }
    }

    private async Task RecordWarning(IUnitOfWork unitOfWork, List<LogEvent> batch)
    {
        if (batch.Count == 0)
        {
            return;
        }

        var logs = new List<WarningLog>();
        foreach (var logEvent in batch)
        {
            var log = new WarningLog
            {
                Id = IdHelper.NextId(),
                CreateTime = logEvent.Timestamp.DateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                MessageTemplate = logEvent.MessageTemplate.Text,
                Properties = logEvent.Properties.ToJson()
            };
            logs.Add(log);
        }

        try
        {
            var sqlSugarScope = unitOfWork.GetDbClient();
            var logDbAttribute = typeof(WarningLog).GetCustomAttribute<LogDataBaseAttribute>();
            if (logDbAttribute != null)
            {
                var sugarClient = sqlSugarScope.GetConnectionScope(App.GetOptions<SystemOptions>().LogDataBase);
                await sugarClient.Insertable(logs).SplitTable().ExecuteCommandAsync();
            }
        }
        catch (Exception e)
        {
            LogHelper.WriteLog(e.ToString(), null);
            ConsoleHelper.WriteLine(e.ToString(), ConsoleColor.Red);
        }
    }

    private async Task RecordError(IUnitOfWork unitOfWork, List<LogEvent> logEvents)
    {
        if (logEvents.Count == 0)
        {
            return;
        }

        var logs = new List<ErrorLog>();
        foreach (var logEvent in logEvents)
        {
            var log = new ErrorLog
            {
                Id = IdHelper.NextId(),
                CreateTime = logEvent.Timestamp.DateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                MessageTemplate = logEvent.MessageTemplate.Text,
                Properties = logEvent.Properties.ToJson(),
            };
            logs.Add(log);
        }

        try
        {
            var sqlSugarScope = unitOfWork.GetDbClient();
            var logDbAttribute = typeof(ErrorLog).GetCustomAttribute<LogDataBaseAttribute>();
            if (logDbAttribute != null)
            {
                var sugarClient = sqlSugarScope.GetConnectionScope(App.GetOptions<SystemOptions>().LogDataBase);
                await sugarClient.Insertable(logs).SplitTable().ExecuteCommandAsync();
            }
        }
        catch (Exception e)
        {
            LogHelper.WriteLog(e.ToString(), null);
            ConsoleHelper.WriteLine(e.ToString(), ConsoleColor.Red);
        }
    }

    private async Task RecordFatal(IUnitOfWork unitOfWork, List<LogEvent> logEvents)
    {
        if (logEvents.Count == 0)
        {
            return;
        }

        var logs = new List<FatalLog>();
        foreach (var logEvent in logEvents)
        {
            var log = new FatalLog
            {
                Id = IdHelper.NextId(),
                CreateTime = logEvent.Timestamp.DateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                MessageTemplate = logEvent.MessageTemplate.Text,
                Properties = logEvent.Properties.ToJson()
            };
            logs.Add(log);
        }

        try
        {
            var sqlSugarScope = unitOfWork.GetDbClient();
            var logDbAttribute = typeof(FatalLog).GetCustomAttribute<LogDataBaseAttribute>();
            if (logDbAttribute != null)
            {
                var sugarClient = sqlSugarScope.GetConnectionScope(App.GetOptions<SystemOptions>().LogDataBase);
                await sugarClient.Insertable(logs).SplitTable().ExecuteCommandAsync();
            }
        }
        catch (Exception e)
        {
            LogHelper.WriteLog(e.ToString(), null);
            ConsoleHelper.WriteLine(e.ToString(), ConsoleColor.Red);
        }
    }


    private async Task RecordSql(IUnitOfWork unitOfWork, List<LogEvent> logEvents)
    {
        if (logEvents.Count == 0)
        {
            return;
        }

        var logs = new List<AopSqlLog>();
        foreach (var logEvent in logEvents)
        {
            var log = new AopSqlLog
            {
                Id = IdHelper.NextId(),
                CreateTime = logEvent.Timestamp.DateTime,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                MessageTemplate = logEvent.MessageTemplate.Text,
                Properties = logEvent.Properties.ToJson()
            };
            logs.Add(log);
        }

        try
        {
            var sqlSugarScope = unitOfWork.GetDbClient();
            var logDbAttribute = typeof(AopSqlLog).GetCustomAttribute<LogDataBaseAttribute>();
            if (logDbAttribute != null)
            {
                var sugarClient = sqlSugarScope.GetConnectionScope(App.GetOptions<SystemOptions>().LogDataBase);
                await sugarClient.Insertable(logs).SplitTable().ExecuteCommandAsync();
            }
        }
        catch (Exception e)
        {
            LogHelper.WriteLog(e.ToString(), null);
            ConsoleHelper.WriteLine(e.ToString(), ConsoleColor.Red);
        }
    }
}