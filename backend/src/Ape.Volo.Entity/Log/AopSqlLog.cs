using Ape.Volo.Entity.Attributes;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// SQL日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_sql"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class AopSqlLog : LogEntityBase
    {
    }
}
