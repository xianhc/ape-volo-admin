using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户密码Dto
/// </summary>
public class UpdateUserPassDto
{
    /// <summary>
    /// 旧密码（RSA 密文的 Base64 串）
    /// </summary>
    [Display(Name = "旧密码")]
    [Required(ErrorMessage = "{0}required")]
    [StringLength(684, MinimumLength = 684)]
    public string OldPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密码（RSA 密文的 Base64 串）
    /// </summary>
    [Display(Name = "新密码")]
    [Required(ErrorMessage = "{0}required")]
    [StringLength(684, MinimumLength = 684)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 确认新密码（RSA 密文的 Base64 串）
    /// </summary>
    [Display(Name = "确认新密码")]
    [Required(ErrorMessage = "{0}required")]
    [StringLength(684, MinimumLength = 684)]
    //[Compare("NewPassword", ErrorMessage = "User.FailedVerificationTwice")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
