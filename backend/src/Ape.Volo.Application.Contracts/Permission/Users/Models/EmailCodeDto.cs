using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 获取邮箱验证码
/// </summary>
public class EmailCodeDto
{
    /// <summary>
    /// 邮箱
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}required")]
    [EmailAddress(ErrorMessage = "{0}Error.Format")]
    public string Email { get; set; } = string.Empty;
}
