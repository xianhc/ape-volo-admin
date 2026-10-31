using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Shared.Enums;

/// <summary>
/// 触发器状态（数值与 Quartz.TriggerState 一致，保证前端序列化兼容）
/// </summary>
public enum TriggerState
{
    /// <summary>
    /// 无触发器
    /// </summary>
    [Display(Name = "无")]
    None = 0,

    /// <summary>
    /// 正常
    /// </summary>
    [Display(Name = "正常")]
    Normal = 1,

    /// <summary>
    /// 已暂停
    /// </summary>
    [Display(Name = "已暂停")]
    Paused = 2,

    /// <summary>
    /// 已完成
    /// </summary>
    [Display(Name = "已完成")]
    Complete = 3,

    /// <summary>
    /// 错误
    /// </summary>
    [Display(Name = "错误")]
    Error = 4,

    /// <summary>
    /// 阻塞
    /// </summary>
    [Display(Name = "阻塞")]
    Blocked = 5
}
