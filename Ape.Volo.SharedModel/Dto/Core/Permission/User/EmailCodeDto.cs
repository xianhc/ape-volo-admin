using System;
using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.SharedModel.Dto.Core.Permission.User;

/// <summary>
/// 获取邮箱验证码
/// </summary>
public class EmailCodeDto
{
    /// <summary>
    /// 邮箱
    /// </summary>
    [Display(Name = "Sys.Email")]
    [Required(ErrorMessage = "{0}required")]
    [EmailAddress(ErrorMessage = "{0}Error.Format")]
    public string Email { get; set; }
}
