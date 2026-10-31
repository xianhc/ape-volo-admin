using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Organization.Models;

/// <summary>
/// 岗位导出模板
/// </summary>
public class PositionExport : ExportBase
{
    /// <summary>
    /// 岗位名称
    /// </summary>
    [Display(Name = "岗位名称")]
    public string Name { get; set; } = string.Empty;

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
