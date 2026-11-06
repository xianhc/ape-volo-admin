using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Operations.Models;

/// <summary>
/// 任务调度Dto
/// </summary>
public sealed record CreateUpdateScheduledJobDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 任务名称
    /// </summary>
    [Display(Name = "任务名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string JobName { get; init; } = string.Empty;

    /// <summary>
    /// 任务分组
    /// </summary>
    [Display(Name = "任务组")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string JobGroup { get; init; } = string.Empty;

    /// <summary>
    /// cron 表达式
    /// </summary>
    [Display(Name = "Cron表达式")]
    public string? Cron { get; init; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    [Display(Name = "命名空间")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string AssemblyName { get; init; } = string.Empty;

    /// <summary>
    /// 任务所在类
    /// </summary>
    [Display(Name = "执行类")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string ClassName { get; init; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// 任务负责人
    /// </summary>
    public string? Principal { get; init; }

    /// <summary>
    /// 告警邮箱
    /// </summary>
    public string? AlertEmail { get; init; }

    /// <summary>
    /// 任务失败后是否继续
    /// </summary>
    public bool PauseAfterFailure { get; init; }


    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// 触发器类型（0、simple 1、cron）
    /// </summary>
    [Display(Name = "任务类型")]
    [Range(1, 2, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public TriggerType TriggerType { get; init; }

    /// <summary>
    /// 执行间隔周期, 秒为单位
    /// </summary>
    public int? IntervalSecond { get; init; }

    /// <summary>
    /// 循环执行次数
    /// </summary>
    public int? CycleRunTimes { get; init; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 执行参数
    /// </summary>
    public string? RunParams { get; init; }
}
