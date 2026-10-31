using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Organization.Models;

/// <summary>
/// 岗位Dto
/// </summary>
public class CreateUpdatePositionDto : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "岗位名称")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    [Range(1, 999, ErrorMessage = "{0}range{1}{2}")]
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
}
