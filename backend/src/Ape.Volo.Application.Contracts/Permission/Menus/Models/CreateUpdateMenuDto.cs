using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Menus.Models;

/// <summary>
/// 菜单Dto
/// </summary>
public class CreateUpdateMenuDto : BaseEntityDto<long>
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "菜单标题")]
    [Required(ErrorMessage = "{0}required")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 路径
    /// </summary>
    [Display(Name = "组件路径")]
    public string? Path { get; set; }

    /// <summary>
    /// 权限标识
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
    /// 父级ID（null 表示顶级菜单）
    /// </summary>
    [Display(Name = "菜单父ID")]
    public long? ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    [Range(1, 999, ErrorMessage = "{0}range{1}{2}")]
    public int Sort { get; set; }

    /// <summary>
    /// Icon图标
    /// </summary>
    [Display(Name = "Icon图标")]
    public string? Icon { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [Display(Name = "菜单类型")]
    [Range(1, 5, ErrorMessage = "{0}range{1}{2}")]
    public MenuType MenuType { get; set; }

    /// <summary>
    /// 缓存
    /// </summary>
    [Display(Name = "是否缓存")]
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Display(Name = "是否启用")]
    public bool Enabled { get; set; }

    /// <summary>
    /// 隐藏
    /// </summary>
    [Display(Name = "是否隐藏")]
    public bool Hidden { get; set; }
}