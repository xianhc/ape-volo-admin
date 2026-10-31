using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Attributes;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 修改用户角色
/// </summary>
public class UpdateUserRoleDto
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [Display(Name = "ID")]
    [RegularExpression(@"^\+?[1-9]\d*$")]
    public long Id { get; set; }

    /// <summary>
    /// 角色Id数组
    /// </summary>
    [Display(Name = "角色")]
    [Required(ErrorMessage = "{0}required")]
    [AtLeastOneItem]
    public List<long> RoleIdArray { get; set; } = new();
}
