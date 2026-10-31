using Ape.Volo.Entity.Attributes;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 错误日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_error"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class ErrorLog : LogEntityBase
    {
    }
}
