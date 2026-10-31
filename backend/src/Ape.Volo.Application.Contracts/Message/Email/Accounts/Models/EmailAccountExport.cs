using System.ComponentModel.DataAnnotations;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;

/// <summary>
/// 邮箱账户导出模板
/// </summary>
public class EmailAccountExport : ExportBase
{
    /// <summary>
    ///邮箱
    /// </summary>
    [Display(Name = "电子邮件")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件显示名称
    /// </summary>
    [Display(Name = "显示名称")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件主机
    /// </summary>
    [Display(Name = "主机")]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮件端口
    /// </summary>
    [Display(Name = "端口")]
    public int Port { get; set; }

    /// <summary>
    /// 电子邮件用户名
    /// </summary>
    [Display(Name = "账户名称")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 是否SSL
    /// </summary>
    [Display(Name = "是否SSL")]
    public bool EnableSsl { get; set; }

    /// <summary>
    /// 是否与请求一起发送应用程序的默认系统凭据
    /// </summary>
    [Display(Name = "使用默认系统凭据")]
    public bool UseDefaultCredentials { get; set; }
}
