using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Model;

namespace Ape.Volo.ViewModel.Report.Permission;

/// <summary>
/// 角色导出模板
/// </summary>
public class RoleExport : ExportBase
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [Display(Name = "Role.Name")]
    public string Name { get; set; }

    /// <summary>
    /// 角色等级
    /// </summary>
    [Display(Name = "Role.Level")]
    public int Level { get; set; }

    /// <summary>
    /// 角色描述
    /// </summary>
    [Display(Name = "Sys.Description")]
    public string Description { get; set; }

    /// <summary>
    /// 数据范围
    /// </summary>
    [Display(Name = "Role.DataScopeType")]
    public DataScopeType DataScopeType { get; set; }

    /// <summary>
    /// 数据部门
    /// </summary>
    [Display(Name = "Role.DataDept")]
    public string DataDept { get; set; }

    /// <summary>
    /// 角色代码
    /// </summary>
    [Display(Name = "Role.Permission")]
    public string AuthCode { get; set; }
}
