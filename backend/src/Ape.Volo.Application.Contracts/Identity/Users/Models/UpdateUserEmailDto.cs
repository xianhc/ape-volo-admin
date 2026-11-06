using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 用户邮箱Dto
/// </summary>
public sealed record UpdateUserEmailDto
{
    /// <summary>
    /// 密码
    /// </summary>
    [Display(Name = "用户密码")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [StringLength(684, MinimumLength = 684)]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [EmailAddress(ErrorMessage = "{0}格式不正确。")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// 验证码
    /// </summary>
    [Display(Name = "验证码")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Code { get; init; } = string.Empty;
}
