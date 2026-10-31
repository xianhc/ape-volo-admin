using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户个人中心Dto
/// </summary>
public class UpdateUserCenterDto
{
    /// <summary>
    /// 昵称
    /// </summary>
    [Display(Name = "用户昵称")]
    [Required(ErrorMessage = "{0}required")]
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 性别
    /// </summary>
    [Display(Name = "性别")]
    [Required(ErrorMessage = "{0}required")]
    public GenderCode GenderCode { get; set; }

    /// <summary>
    /// 电话
    /// </summary>
    [Display(Name = "电话")]
    [RegularExpression(@"^(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\d{8}$",
        ErrorMessage = "{0}Error.Format")]
    public string? Phone { get; set; }
}
