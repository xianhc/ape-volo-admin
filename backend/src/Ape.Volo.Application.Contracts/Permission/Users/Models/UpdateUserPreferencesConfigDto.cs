using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Permission.Users.Models;

/// <summary>
/// 用户偏好设置Dto
/// </summary>
public class UpdateUserPreferencesConfigDto
{
    /// <summary>
    /// 偏好设置
    /// </summary>
    [Display(Name = "偏好设置")]
    [Required(ErrorMessage = "{0}required")]
    public string PreferencesConfig { get; set; } = string.Empty;
}
