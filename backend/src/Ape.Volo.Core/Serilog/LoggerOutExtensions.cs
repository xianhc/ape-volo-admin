using Serilog;
using Serilog.Events;
using Serilog.Filters;
using SqlSugar;

namespace Ape.Volo.Core.Serilog;

/// <summary>
/// 日志输出扩展
/// </summary>
public static class LoggerOutExtensions
{
    public static LoggerConfiguration RecordSql(this LoggerConfiguration lc, string propertyName)
    {
        lc = lc.Filter.ByIncludingOnly(Matching.WithProperty<bool>(LoggerProperty.RecordSqlLog,
            s => s)).Filter.ByIncludingOnly(Matching.WithProperty<bool>(
            propertyName, s => s));
        return lc;
    }

    public static List<LogEvent> RecordSql(this List<LogEvent> batch)
    {
        //只记录 Insert、Update、Delete语句
        return batch.Where(s =>
                s.WithProperty<bool>(LoggerProperty.RecordSqlLog, q => q) &&
                s.WithProperty<bool>(LoggerProperty.ToDb, q => q))
            .Where(s => s.WithProperty<SugarActionType>(LoggerProperty.SugarActionType,
                q => !new[] { SugarActionType.UnKnown, SugarActionType.Query }.Contains(q))).ToList();
    }

    public static LoggerConfiguration RecordLog(this LoggerConfiguration lc)
    {
        lc = lc.Filter.ByIncludingOnly(WithProperty<bool>(LoggerProperty.RecordSqlLog,
            s => !s));
        return lc;
    }

    public static List<LogEvent> RecordLog(this IEnumerable<LogEvent> batch)
    {
        return batch.Where(s => WithProperty<bool>(LoggerProperty.RecordSqlLog,
            q => !q)(s)).ToList();
    }

    public static Func<LogEvent, bool> WithProperty<T>(string propertyName, Func<T, bool> predicate)
    {
        return e =>
        {
            if (!e.Properties.TryGetValue(propertyName, out var propertyValue)) return true;

            return propertyValue is ScalarValue { Value: T value } && predicate(value);
        };
    }

    public static bool WithProperty<T>(this LogEvent e, string key, Func<T, bool> predicate)
    {
        if (!e.Properties.TryGetValue(key, out var propertyValue)) return false;

        return propertyValue is ScalarValue { Value: T value } && predicate(value);
    }
}
