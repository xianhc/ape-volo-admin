using System.Net.Mail;

namespace Ape.Volo.Core.Message.Email;

/// <summary>
/// SMTP 客户端工厂：按邮件账号创建已配置好的 <see cref="SmtpClient"/>
/// </summary>
public interface ISmtpClientFactory
{
    /// <summary>
    /// 为指定邮件账号创建 SMTP 客户端
    /// </summary>
    /// <param name="emailAccountId">邮件账号标识，为空时使用系统默认账号</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>已完成主机、端口、SSL 与凭据配置的 SMTP 客户端</returns>
    Task<SmtpClient> CreateAsync(long? emailAccountId = null, CancellationToken ct = default);
}
