using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Permission.Departments.Models;

/// <summary>
/// 部门Dto
/// </summary>
public class CreateUpdateDepartmentDto : BaseEntityDto<long>
{
    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "名称")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 父级ID（null 表示顶级部门）
    /// </summary>
    public long? ParentId { get; set; }

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
