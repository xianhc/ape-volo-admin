using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.System;

namespace Ape.Volo.SharedModel.Dto.Core.System;

/// <summary>
/// 系统配置Dto
/// </summary>
[AutoMapping(typeof(Setting), typeof(CreateUpdateSettingDto))]
public class CreateUpdateSettingDto : BaseEntityDto<long>
{
    /// <summary>
    /// 键
    /// </summary>
    [Display(Name = "Setting.Name")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    [Display(Name = "Setting.Value")]
    [Required(ErrorMessage = "{0}required")]
    public string Value { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "Sys.Description")]
    public string Description { get; set; }
}