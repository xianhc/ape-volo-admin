using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户邮箱Dto
/// </summary>
public class UpdateUserEmailDto
{
    /// <summary>
    /// 密码
    /// </summary>
    [Display(Name = "用户密码")]
    [Required(ErrorMessage = "{0}required")]
    [StringLength(684, MinimumLength = 684)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}required")]
    [EmailAddress(ErrorMessage = "{0}Error.Format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 验证码
    /// </summary>
    [Display(Name = "验证码")]
    [Required(ErrorMessage = "{0}required")]
    public string Code { get; set; } = string.Empty;
}
