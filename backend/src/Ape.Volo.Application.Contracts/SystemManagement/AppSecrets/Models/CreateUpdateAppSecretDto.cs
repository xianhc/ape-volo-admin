using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.SystemManagement.AppSecrets.Models;

/// <summary>
/// 应用密钥Dto
/// </summary>
public class CreateUpdateAppSecretDto : BaseEntityDto<long>
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
    [Display(Name = "应用名称")]
    [Required(ErrorMessage = "{0}required")]
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}
