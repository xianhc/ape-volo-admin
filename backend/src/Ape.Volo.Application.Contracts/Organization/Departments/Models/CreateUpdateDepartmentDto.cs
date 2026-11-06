using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Organization.Departments.Models;

/// <summary>
/// 部门Dto
/// </summary>
public sealed record CreateUpdateDepartmentDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 名称
    /// </summary>
    [Display(Name = "名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 父级ID（null 表示顶级部门）
    /// </summary>
    public long? ParentId { get; init; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    [Range(1, 999, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public int Sort { get; init; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }
}
