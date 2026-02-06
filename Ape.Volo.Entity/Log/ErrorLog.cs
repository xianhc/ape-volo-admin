using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 错误日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_error"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class ErrorLog : SerilogBase
    {
    }
}
