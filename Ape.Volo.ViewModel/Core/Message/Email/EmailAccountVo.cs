using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.Message.Email;

namespace Ape.Volo.ViewModel.Core.Message.Email;

/// <summary>
/// 邮箱账户Vo
/// </summary>
[AutoMapping(typeof(EmailAccount), typeof(EmailAccountVo))]
public class EmailAccountVo : BaseEntityDto<long>
{
    /// <summary>
    ///电子邮件地址
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 电子邮件显示名称
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// 电子邮件主机
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// 电子邮件端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 电子邮件用户名
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// 电子邮件密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 是否SSL
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// 是否与请求一起发送应用程序的默认系统凭据
    /// </summary>
    public bool UseDefaultCredentials { get; set; }
}
