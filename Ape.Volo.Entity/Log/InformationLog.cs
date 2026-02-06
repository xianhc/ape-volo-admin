using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 信息日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_information"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class InformationLog : SerilogBase
    {
    }
}
