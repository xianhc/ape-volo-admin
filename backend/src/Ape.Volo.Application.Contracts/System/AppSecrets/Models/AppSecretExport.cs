using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.AppSecrets.Models;

/// <summary>
/// 密钥导出模板
/// </summary>
public class AppSecretExport : ExportBase
{
    /// <summary>
    /// 应用ID
    /// </summary>
    [Display(Name = "应用ID")]
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 应用名称
    /// </summary>
    [Display(Name = "应用名称")]
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    [Display(Name = "备注")]
    public string? Remark { get; set; }
}
