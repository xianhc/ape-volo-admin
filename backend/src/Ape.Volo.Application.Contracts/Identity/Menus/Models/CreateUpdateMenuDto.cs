using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Identity.Menus.Models;

/// <summary>
/// 菜单Dto
/// </summary>
public sealed record CreateUpdateMenuDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "菜单标题")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// 路径
    /// </summary>
    [Display(Name = "组件路径")]
    public string? Path { get; init; }

    /// <summary>
    /// 权限标识
    /// </summary>
    [Display(Name = "权限标识符")]
    public string? AuthCode { get; init; }


    /// <summary>
    /// 组件
    /// </summary>
    [Display(Name = "组件")]
    public string? Component { get; init; }

    /// <summary>
    /// 组件名称
    /// </summary>
    [Display(Name = "组件名称")]
    public string? ComponentName { get; init; }

    /// <summary>
    /// 父级ID（null 表示顶级菜单）
    /// </summary>
    [Display(Name = "菜单父ID")]
    public long? ParentId { get; init; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    [Range(1, 999, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public int Sort { get; init; }

    /// <summary>
    /// Icon图标
    /// </summary>
    [Display(Name = "Icon图标")]
    public string? Icon { get; init; }

    /// <summary>
    /// 类型
    /// </summary>
    [Display(Name = "菜单类型")]
    [Range(1, 5, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public MenuType MenuType { get; init; }

    /// <summary>
    /// 缓存
    /// </summary>
    [Display(Name = "是否缓存")]
    public bool KeepAlive { get; init; }

    /// <summary>
    /// 状态
    /// </summary>
    [Display(Name = "是否启用")]
    public bool Enabled { get; init; }

    /// <summary>
    /// 隐藏
    /// </summary>
    [Display(Name = "是否隐藏")]
    public bool Hidden { get; init; }
}