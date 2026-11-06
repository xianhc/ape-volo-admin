using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.System.AppSecrets.Models;

/// <summary>
/// 应用密钥Dto
/// </summary>
public sealed record CreateUpdateAppSecretDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 应用ID
    /// </summary>
    public string AppId { get; init; } = string.Empty;

    /// <summary>
    /// 签名Key
    /// </summary>
    public string AppSecretKey { get; init; } = string.Empty;

    /// <summary>
    /// 应用名称
    /// </summary>
    [Display(Name = "应用名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string AppName { get; init; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; init; }
}
