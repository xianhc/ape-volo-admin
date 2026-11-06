using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 用户角色Dto
/// </summary>
public class UserRoleDto
{
    /// <summary>
    /// ID
    /// </summary>
    [Display(Name = "ID")]
    [RegularExpression(@"^\+?[1-9]\d*$", ErrorMessage = "{0}格式不正确。")]

    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Name { get; set; } = string.Empty;
}
