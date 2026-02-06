using Ape.Volo.Core.ConfigOptions;
using Serilog.Context;
using SqlSugar;

namespace Ape.Volo.Core.Serilog;

public class LoggerPropertyConfiguration : IDisposable
{
    private readonly Stack<IDisposable> _disposableStack = new();

    public static LoggerPropertyConfiguration Create => new();

    public void AddStock(IDisposable disposable)
    {
        _disposableStack.Push(disposable);
    }

    public IDisposable AddAopSqlProperty(ISqlSugarClient db, SerilogOptions serilogOptions)
    {
        AddStock(LogContext.PushProperty(LoggerProperty.RecordSqlLog, serilogOptions.RecordSql));
        AddStock(LogContext.PushProperty(LoggerProperty.ToDb, serilogOptions.ToDb));
        AddStock(LogContext.PushProperty(LoggerProperty.ToFile, serilogOptions.ToFile));
        AddStock(LogContext.PushProperty(LoggerProperty.ToConsole, serilogOptions.ToConsole));
        AddStock(LogContext.PushProperty(LoggerProperty.ToElasticsearch, serilogOptions.ToElasticsearch));
        AddStock(LogContext.PushProperty(LoggerProperty.SugarActionType, db.SugarActionType));
        return this;
    }


    public void Dispose()
    {
        while (_disposableStack.Count > 0)
        {
            _disposableStack.Pop().Dispose();
        }
    }
}