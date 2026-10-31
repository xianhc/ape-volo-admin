using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户角色Dto
/// </summary>
public class UserRoleDto
{
    /// <summary>
    /// ID
    /// </summary>
    [Display(Name = "ID")]
    [RegularExpression(@"^\+?[1-9]\d*$", ErrorMessage = "{0}Error.Format")]

    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "名称")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; } = string.Empty;
}
