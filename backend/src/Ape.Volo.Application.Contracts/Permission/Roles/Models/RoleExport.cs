using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Application.Contracts.Permission.Roles.Models;

/// <summary>
/// 角色导出模板
/// </summary>
public class RoleExport : ExportBase
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Display(Name = "角色名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    [Display(Name = "描述")]
    public string? Description { get; set; }

    /// <summary>
    /// 数据范围
    /// </summary>
    [Display(Name = "数据权限类型")]
    public DataScopeType DataScopeType { get; set; }

    /// <summary>
    /// 数据部门
    /// </summary>
    [Display(Name = "数据部门")]
    public string? DataDept { get; set; }

    /// <summary>
    /// 角色代码
    /// </summary>
    [Display(Name = "权限标识")]
    public string AuthCode { get; set; } = string.Empty;
}
