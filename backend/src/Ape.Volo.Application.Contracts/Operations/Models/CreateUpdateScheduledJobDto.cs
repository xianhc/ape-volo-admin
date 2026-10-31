using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Operations.Models;

/// <summary>
/// 任务调度Dto
/// </summary>
public class CreateUpdateScheduledJobDto : BaseEntityDto<long>
{
    /// <summary>
    /// 任务名称
    /// </summary>
    [Display(Name = "任务名称")]
    [Required(ErrorMessage = "{0}required")]
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// 任务分组
    /// </summary>
    [Display(Name = "任务组")]
    [Required(ErrorMessage = "{0}required")]
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
    [Required(ErrorMessage = "{0}required")]
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// 任务所在类
    /// </summary>
    [Display(Name = "执行类")]
    [Required(ErrorMessage = "{0}required")]
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 任务负责人
    /// </summary>
    public string? Principal { get; set; }

    /// <summary>
    /// 告警邮箱
    /// </summary>
    public string? AlertEmail { get; set; }

    /// <summary>
    /// 任务失败后是否继续
    /// </summary>
    public bool PauseAfterFailure { get; set; }


    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// 触发器类型（0、simple 1、cron）
    /// </summary>
    [Display(Name = "任务类型")]
    [Range(1, 2, ErrorMessage = "{0}range{1}{2}")]
    public TriggerType TriggerType { get; set; }

    /// <summary>
    /// 执行间隔周期, 秒为单位
    /// </summary>
    public int? IntervalSecond { get; set; }

    /// <summary>
    /// 循环执行次数
    /// </summary>
    public int? CycleRunTimes { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 执行参数
    /// </summary>
    public string? RunParams { get; set; }
}
