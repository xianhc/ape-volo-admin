using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;

/// <summary>
/// 邮箱账户Dto
/// </summary>
public sealed record CreateUpdateEmailAccountDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    ///电子邮件地址
    /// </summary>
    [Display(Name = "邮箱")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [EmailAddress(ErrorMessage = "{0}格式不正确。")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// 电子邮件显示名称
    /// </summary>
    [Display(Name = "显示名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// 电子邮件主机
    /// </summary>
    [Display(Name = "主机")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// 电子邮件端口
    /// </summary>
    [Display(Name = "端口")]
    [Range(1, 65535, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public int Port { get; init; }

    /// <summary>
    /// 电子邮件用户名
    /// </summary>
    [Display(Name = "账户名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// 电子邮件密码
    /// </summary>
    [Display(Name = "密码")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// 是否SSL
    /// </summary>
    public bool EnableSsl { get; init; } = false;

    /// <summary>
    /// 是否与请求一起发送应用程序的默认系统凭据
    /// </summary>
    public bool UseDefaultCredentials { get; init; } = false;
}
