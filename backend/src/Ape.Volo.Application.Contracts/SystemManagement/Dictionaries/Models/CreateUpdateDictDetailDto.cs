using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.SystemManagement.Dictionaries.Models;

/// <summary>
/// 字典详情Dto
/// </summary>
public class CreateUpdateDictDetailDto : BaseEntityDto<long>
{
    /// <summary>
    /// 字典ID
    /// </summary>
    [Display(Name = "字典ID")]
    [Required(ErrorMessage = "{0}required")]
    public long DictId { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [Display(Name = "字典标签")]
    [Required(ErrorMessage = "{0}required")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [Display(Name = "字典值")]
    [Required(ErrorMessage = "{0}required")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    [Range(1, 999, ErrorMessage = "{0}range{1}{2}")]
    public int DictSort { get; set; }
}
