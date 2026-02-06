using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 警告日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_warning"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class WarningLog : SerilogBase
    {
    }
}
