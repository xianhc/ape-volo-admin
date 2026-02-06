using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log
{
    /// <summary>
    /// 失败日志
    /// </summary>
    [LogDataBase]
    [SplitTable(SplitType.Month)]
    [SugarTable($@"{"log_fatal"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
    public class FatalLog : SerilogBase
    {
    }
}
