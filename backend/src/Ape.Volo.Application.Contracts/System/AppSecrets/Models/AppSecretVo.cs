namespace Ape.Volo.Application.Contracts.System.AppSecrets.Models;

/// <summary>
/// 应用密钥Vo
/// </summary>
public class AppSecretVo : BaseEntityDto<long>
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 签名Key
    /// </summary>
    public string AppSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 应用名称
    /// </summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}
