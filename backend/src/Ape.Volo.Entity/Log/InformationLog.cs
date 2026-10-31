using Ape.Volo.Entity.Attributes;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 信息日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_information"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class InformationLog : LogEntityBase
    {
    }
}
