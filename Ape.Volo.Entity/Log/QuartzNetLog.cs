using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Log;


/// <summary>
/// 作业日志
/// </summary>
[LogDataBase]
[SplitTable(SplitType.Month)]
[SugarTable($@"{"log_quartz_net"}_{{year}}{{month}}{{day}}", IsDisabledUpdateAll = true)]
public class QuartzNetLog : BaseEntity
{
    /// <summary>
    /// 任务Id
    /// </summary>
    public long TaskId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string? TaskName { get; set; }

    /// <summary>
    /// 任务分组
    /// </summary>
    public string? TaskGroup { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string? AssemblyName { get; set; }

    /// <summary>
    /// 任务所在类
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// cron 表达式
    /// </summary>
    public string? Cron { get; set; }

    /// <summary>
    /// 异常详情
    /// </summary>
    public string? ExceptionDetail { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    [SugarColumn(DefaultValue = "0")]
    public long ExecutionDuration { get; set; }

    /// <summary>
    /// 执行传参
    /// </summary>
    public string? RunParams { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
}
