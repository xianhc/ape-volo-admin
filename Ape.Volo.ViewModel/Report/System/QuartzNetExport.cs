using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Model;

namespace Ape.Volo.ViewModel.Report.System;

/// <summary>
/// 任务调度导出模板
/// </summary>
public class QuartzNetExport : ExportBase
{
    /// <summary>
    /// 任务名称
    /// </summary>
    [Display(Name = "Task.TaskName")]
    public string TaskName { get; set; }

    /// <summary>
    /// 任务分组
    /// </summary>
    [Display(Name = "Task.TaskGroup")]
    public string TaskGroup { get; set; }

    /// <summary>
    /// cron 表达式
    /// </summary>
    [Display(Name = "Task.Cron")]
    public string Cron { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    [Display(Name = "Task.AssemblyName")]
    public string AssemblyName { get; set; }

    /// <summary>
    /// 任务所在类
    /// </summary>
    [Display(Name = "Task.ClassName")]
    public string ClassName { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    [Display(Name = "Sys.Description")]
    public string Description { get; set; }

    /// <summary>
    /// 任务负责人
    /// </summary>
    [Display(Name = "Task.Principal")]
    public string Principal { get; set; }

    /// <summary>
    /// 告警邮箱
    /// </summary>
    [Display(Name = "Task.AlertEmail")]
    public string AlertEmail { get; set; }

    /// <summary>
    /// 任务失败后是否继续
    /// </summary>
    [Display(Name = "Task.PauseAfterFailure")]
    public bool PauseAfterFailure { get; set; }

    /// <summary>
    /// 执行次数
    /// </summary>
    [Display(Name = "Task.RunTimes")]
    public int RunTimes { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    [Display(Name = "Task.StartTime")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [Display(Name = "Task.EndTime")]
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 触发器类型（0、simple 1、cron）
    /// </summary>
    [Display(Name = "Task.TriggerType")]
    public TriggerType TriggerType { get; set; }

    /// <summary>
    /// 执行间隔时间, 秒为单位
    /// </summary>
    [Display(Name = "Task.IntervalSecond")]
    public int? IntervalSecond { get; set; }

    /// <summary>
    /// 循环执行次数
    /// </summary>
    [Display(Name = "Task.CycleRunTimes")]
    public int? CycleRunTimes { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Display(Name = "Task.IsEnable")]
    public bool Enabled { get; set; }

    /// <summary>
    /// 执行传参
    /// </summary>
    [Display(Name = "Task.RunParams")]
    public string RunParams { get; set; }

    /// <summary>
    /// 触发器状态
    /// </summary>
    [Display(Name = "Task.TriggerStatus")]
    public string TriggerStatus { get; set; }
}
