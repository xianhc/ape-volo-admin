using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;

/// <summary>
/// 邮箱账户Dto
/// </summary>
public class CreateUpdateEmailAccountDto : BaseEntityDto<long>
{
    /// <summary>
    ///电子邮件地址
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}required")]
    [EmailAddress(ErrorMessage = "{0}Error.Format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件显示名称
    /// </summary>
    [Display(Name = "显示名称")]
    [Required(ErrorMessage = "{0}required")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件主机
    /// </summary>
    [Display(Name = "主机")]
    [Required(ErrorMessage = "{0}required")]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件端口
    /// </summary>
    [Display(Name = "端口")]
    [Range(1, 65535, ErrorMessage = "{0}range{1}{2}")]
    public int Port { get; set; }

    /// <summary>
    /// 电子邮件用户名
    /// </summary>
    [Display(Name = "账户名称")]
    [Required(ErrorMessage = "{0}required")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件密码
    /// </summary>
    [Display(Name = "密码")]
    [Required(ErrorMessage = "{0}required")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 是否SSL
    /// </summary>
    public bool EnableSsl { get; set; } = false;

    /// <summary>
    /// 是否与请求一起发送应用程序的默认系统凭据
    /// </summary>
    public bool UseDefaultCredentials { get; set; } = false;
}
