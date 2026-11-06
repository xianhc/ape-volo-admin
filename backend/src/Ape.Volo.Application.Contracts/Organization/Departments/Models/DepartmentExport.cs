using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Organization.Departments.Models;

/// <summary>
/// 部门导出模板
/// </summary>
public class DepartmentExport : ExportBase
{
    /// <summary>
    /// 部门名称
    /// </summary>
    [Display(Name = "部门名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 部门父ID
    /// </summary>
    [Display(Name = "部门父ID")]
    public long? ParentId { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [Display(Name = "排序")]
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Display(Name = "是否启用")]
    public bool Enabled { get; set; }
}
