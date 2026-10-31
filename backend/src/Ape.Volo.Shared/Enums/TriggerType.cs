using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Shared.Enums;

/// <summary>
/// 触发器类型
/// </summary>
public enum TriggerType
{
    /// <summary>
    /// 表达式
    /// </summary>
    [Display(Name = "Cron模式")]
    Cron = 1,

    /// <summary>
    /// 简单的
    /// </summary>
    [Display(Name = "Simple模式")]
    Simple = 2
}
