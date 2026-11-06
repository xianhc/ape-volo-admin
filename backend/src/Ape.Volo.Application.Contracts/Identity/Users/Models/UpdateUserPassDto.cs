using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 用户密码Dto
/// </summary>
public sealed record UpdateUserPassDto
{
    /// <summary>
    /// 旧密码（RSA 密文的 Base64 串）
    /// </summary>
    [Display(Name = "旧密码")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [StringLength(684, MinimumLength = 684)]
    public string OldPassword { get; init; } = string.Empty;

    /// <summary>
    /// 新密码（RSA 密文的 Base64 串）
    /// </summary>
    [Display(Name = "新密码")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [StringLength(684, MinimumLength = 684)]
    public string NewPassword { get; init; } = string.Empty;

    /// <summary>
    /// 确认新密码（RSA 密文的 Base64 串）
    /// </summary>
    [Display(Name = "确认新密码")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [StringLength(684, MinimumLength = 684)]
    //[Compare("NewPassword", ErrorMessage = "{0}与{1}不一致。")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
