using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Menus.Models;

/// <summary>
/// 菜单导出模板
/// </summary>
public class MenuExport : ExportBase
{
    /// <summary>
    /// 菜单标题
    /// </summary>
    [Display(Name = "菜单标题")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 组件路径
    /// </summary>
    [Display(Name = "组件路径")]
    public string? Path { get; set; }

    /// <summary>
    /// 权限标识符
    /// </summary>
    [Display(Name = "权限标识符")]
    public string? AuthCode { get; set; }

    /// <summary>
    /// 组件
    /// </summary>
    [Display(Name = "组件")]
    public string? Component { get; set; }

    /// <summary>
    /// 组件名称
    /// </summary>
    [Display(Name = "组件名称")]
    public string? ComponentName { get; set; }

    /// <summary>
    /// 菜单父ID
    /// </summary>
    [Display(Name = "菜单父ID")]
    public long? PId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "菜单标题")]
    public int Sort { get; set; }

    /// <summary>
    /// Icon图标
    /// </summary>
    [Display(Name = "Icon图标")]
    public string? Icon { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    [Display(Name = "菜单类型")]
    public MenuType MenuType { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    [Display(Name = "是否缓存")]
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    [Display(Name = "是否隐藏")]
    public bool Hidden { get; set; }
}
