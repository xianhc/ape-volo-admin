using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;

namespace Ape.Volo.Application.Contracts.Permission.Roles.Models;

/// <summary>
/// 更新角色菜单
/// </summary>
public class UpdateRoleMenuDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [Display(Name = "ID")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }

    /// <summary>
    /// 角色菜单
    /// </summary>
    [Display(Name = "菜单")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<long> MenuIdArray { get; set; } = new();
}
