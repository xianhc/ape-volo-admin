using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Shared.Enums;

/// <summary>
/// 菜单类型
/// </summary>
public enum MenuType
{
    /// <summary>
    /// 目录
    /// </summary>
    [Display(Name = "目录")]
    Catalog = 1,

    /// <summary>
    /// 菜单
    /// </summary>
    [Display(Name = "菜单")]
    Menu = 2,

    /// <summary>
    /// 按钮
    /// </summary>
    [Display(Name = "按钮")]
    Button = 3,

    /// <summary>
    /// 内链
    /// </summary>
    InternalLink = 4,

    /// <summary>
    /// 外链
    /// </summary>
    ExternalLink = 5
}
