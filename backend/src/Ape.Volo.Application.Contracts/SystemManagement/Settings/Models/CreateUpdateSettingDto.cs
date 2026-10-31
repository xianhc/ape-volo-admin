using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.SystemManagement.Settings.Models;

/// <summary>
/// 系统配置Dto
/// </summary>
public class CreateUpdateSettingDto : BaseEntityDto<long>
{
    /// <summary>
    /// 键
    /// </summary>
    [Display(Name = "参数名称")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [Display(Name = "参数值")]
    [Required(ErrorMessage = "{0}required")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "描述")]
    public string? Description { get; set; }
}