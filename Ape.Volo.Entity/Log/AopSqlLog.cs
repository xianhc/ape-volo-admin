using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// SQL日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_sql"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class AopSqlLog : SerilogBase
    {
    }
}
