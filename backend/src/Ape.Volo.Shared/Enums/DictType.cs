using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Shared.Enums;

/// <summary>
/// 字典类型
/// </summary>
public enum DictType
{
    /// <summary>
    /// 系统类
    /// </summary>
    [Display(Name = "系统类")]
    System = 1,

    /// <summary>
    /// 业务类
    /// </summary>
    [Display(Name = "业务类")]
    Business = 2
}
