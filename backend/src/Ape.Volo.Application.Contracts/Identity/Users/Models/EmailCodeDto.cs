using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 获取邮箱验证码
/// </summary>
public class EmailCodeDto
{
    /// <summary>
    /// 邮箱
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [EmailAddress(ErrorMessage = "{0}格式不正确。")]
    public string Email { get; set; } = string.Empty;
}
