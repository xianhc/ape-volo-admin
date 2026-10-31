using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.SystemManagement.Dictionaries.Models;

/// <summary>
/// 字典Dto
/// </summary>
public class CreateUpdateDictDto : BaseEntityDto<long>
{
    /// <summary>
    /// 字典类型
    /// </summary>
    [Display(Name = "字典类型")]
    [Range(1, 2, ErrorMessage = "{0}range{1}{2}")]
    public DictType DictType { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "字典名称")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "字典描述")]
    [Required(ErrorMessage = "{0}required")]
    public string Description { get; set; } = string.Empty;
}
