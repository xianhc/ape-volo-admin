using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 用户偏好设置Dto
/// </summary>
public sealed record UpdateUserPreferencesConfigDto
{
    /// <summary>
    /// 偏好设置
    /// </summary>
    [Display(Name = "偏好设置")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string PreferencesConfig { get; init; } = string.Empty;
}
