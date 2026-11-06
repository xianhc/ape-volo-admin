using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.Settings.Models;

/// <summary>
/// 设置导出模板
/// </summary>
public class SettingExport : ExportBase
{
    /// <summary>
    /// 键
    /// </summary>
    [Display(Name = "参数名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    [Display(Name = "参数值")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    [Display(Name = "是否启用")]
    public bool Enabled { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [Display(Name = "描述")]
    public string? Description { get; set; }
}
