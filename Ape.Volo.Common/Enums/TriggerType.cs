using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Common.Enums;

public enum TriggerType
{
    /// <summary>
    /// 表达式
    /// </summary>
    [Display(Name = "Enum.Trigger.Cron")]
    Cron = 1,

    /// <summary>
    /// 简单的
    /// </summary>
    [Display(Name = "Enum.Trigger.Simple")]
    Simple = 2
}
