using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;

namespace Ape.Volo.Application.Contracts.Identity.Roles.Models;

/// <summary>
/// 更新角色菜单
/// </summary>
public sealed record UpdateRoleMenuDto
{
    /// <summary>
    /// 角色ID
    /// </summary>
    [Display(Name = "ID")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; init; }

    /// <summary>
    /// 角色菜单
    /// </summary>
    [Display(Name = "菜单")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [AtLeastOneItem]
    public List<long> MenuIdArray { get; init; } = new();
}
