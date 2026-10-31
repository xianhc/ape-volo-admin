using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Operations.Models;

/// <summary>
/// 任务调度导出模板
/// </summary>
public class ScheduledJobExport : ExportBase
{
    /// <summary>
    /// 任务名称
    /// </summary>
    [Display(Name = "任务名称")]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务分组
    /// </summary>
    [Display(Name = "任务组")]
    public string JobGroup { get; set; } = string.Empty;

    /// <summary>
    /// cron 表达式
    /// </summary>
    [Display(Name = "Cron表达式")]
    public string? Cron { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    [Display(Name = "命名空间")]
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// 任务所在类
    /// </summary>
    [Display(Name = "执行类")]
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    [Display(Name = "描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 任务负责人
    /// </summary>
    [Display(Name = "负责人")]
    public string? Principal { get; set; }

    /// <summary>
    /// 告警邮箱
    /// </summary>
    [Display(Name = "告警邮箱")]
    public string? AlertEmail { get; set; }

    /// <summary>
    /// 任务失败后是否继续
    /// </summary>
    [Display(Name = "是否是否继续")]
    public bool PauseAfterFailure { get; set; }


    /// <summary>
    /// 开始时间
    /// </summary>
    [Display(Name = "开始时间")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Display(Name = "结束时间")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 触发器类型（0、simple 1、cron）
    /// </summary>
    [Display(Name = "任务类型")]
    public TriggerType TriggerType { get; set; }

    /// <summary>
    /// 执行间隔时间, 秒为单位
    /// </summary>
    [Display(Name = "执行间隔时间")]
    public int? IntervalSecond { get; set; }

    /// <summary>
    /// 循环执行次数
    /// </summary>
    [Display(Name = "循环执行次数")]
    public int? CycleRunTimes { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Display(Name = "是否启动")]
    public bool Enabled { get; set; }

    /// <summary>
    /// 执行传参
    /// </summary>
    [Display(Name = "执行传参")]
    public string? RunParams { get; set; }

    /// <summary>
    /// 触发器状态
    /// </summary>
    [Display(Name = "触发器状态")]
    public string? TriggerStatus { get; set; }
}
