using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Model;

namespace Ape.Volo.ViewModel.Report.Permission;

/// <summary>
/// 菜单导出模板
/// </summary>
public class MenuExport : ExportBase
{
    /// <summary>
    /// 菜单标题
    /// </summary>
    [Display(Name = "Menu.Title")]
    public string Title { get; set; }

    /// <summary>
    /// 组件路径
    /// </summary>
    [Display(Name = "Menu.Path")]
    public string Path { get; set; }

    /// <summary>
    /// 权限标识符
    /// </summary>
    [Display(Name = "Menu.Permission")]
    public string AuthCode { get; set; }

    /// <summary>
    /// 组件
    /// </summary>
    [Display(Name = "Menu.Component")]
    public string Component { get; set; }

    /// <summary>
    /// 组件名称
    /// </summary>
    [Display(Name = "Menu.ComponentName")]
    public string ComponentName { get; set; }

    /// <summary>
    /// 菜单父ID
    /// </summary>
    [Display(Name = "Menu.PId")]
    public long PId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "Menu.Title")]
    public int Sort { get; set; }

    /// <summary>
    /// Icon图标
    /// </summary>
    [Display(Name = "Menu.Icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    [Display(Name = "Menu.MenuType")]
    public MenuType MenuType { get; set; }

    /// <summary>
    /// 是否缓存
    /// </summary>
    [Display(Name = "Menu.IsCache")]
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    [Display(Name = "Menu.IsHidden")]
    public bool Hidden { get; set; }

    /// <summary>
    /// 子菜单个数
    /// </summary>
    [Display(Name = "Menu.SubCount")]
    public int SubCount { get; set; }
}
