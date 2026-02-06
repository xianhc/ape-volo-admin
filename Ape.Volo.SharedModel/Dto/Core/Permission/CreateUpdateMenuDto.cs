using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.Permission;

namespace Ape.Volo.SharedModel.Dto.Core.Permission;

/// <summary>
/// 菜单Dto
/// </summary>
[AutoMapping(typeof(Menu), typeof(CreateUpdateMenuDto))]
public class CreateUpdateMenuDto : BaseEntityDto<long>
{
    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "Menu.Title")]
    [Required(ErrorMessage = "{0}required")]
    public string Title { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    [Display(Name = "Menu.Path")]
    public string Path { get; set; }

    /// <summary>
    /// 权限标识
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
    /// 父级ID
    /// </summary>
    [Display(Name = "Menu.PId")]
    public long ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "Sys.Sort")]
    [Range(1, 999, ErrorMessage = "{0}range{1}{2}")]
    public int Sort { get; set; }

    /// <summary>
    /// Icon图标
    /// </summary>
    [Display(Name = "Menu.Icon")]
    public string Icon { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [Display(Name = "Menu.Type")]
    [Range(1, 5, ErrorMessage = "{0}range{1}{2}")]
    public MenuType MenuType { get; set; }

    /// <summary>
    /// 缓存
    /// </summary>
    [Display(Name = "Menu.KeepAlive")]
    public bool KeepAlive { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Display(Name = "Menu.Enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// 隐藏
    /// </summary>
    [Display(Name = "Menu.Hidden")]
    public bool Hidden { get; set; }

    /// <summary>
    /// 徽章类型
    /// </summary>
    public BadgeType? BadgeType { get; set; }

    /// <summary>
    /// 徽章文字
    /// </summary>
    public string BadgeText { get; set; }

    /// <summary>
    /// 徽章样式
    /// </summary>
    public string BadgeStyle { get; set; }
}