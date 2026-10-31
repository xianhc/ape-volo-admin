using System.Net.Mail;
using Ape.Volo.Application.Contracts.Message.Email;
using System.Text;
using Microsoft.Extensions.Logging;
using Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;

namespace Ape.Volo.Core.Message.Email;

/// <summary>
/// 邮件发送（基于框架内置 <see cref="SmtpClient"/>，不依赖 MailKit）
/// </summary>
public class EmailSender : IEmailSender
{
    #region Fields

    private readonly ISmtpClientFactory _smtpClientFactory;
    private readonly ILogger<EmailSender> _logger;

    #endregion

    #region Ctor

    /// <summary>
    /// 
    /// </summary>
    /// <param name="smtpClientFactory"></param>
    /// <param name="logger">当前宿主日志</param>
    public EmailSender(ISmtpClientFactory smtpClientFactory, ILogger<EmailSender> logger)
    {
        _smtpClientFactory = smtpClientFactory;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<bool> SendEmailAsync(EmailAccountVo emailAccount, string subject, string body,
        string fromAddress, string fromName, string toAddress, string toName,
        string? replyToAddress = null, string? replyToName = null,
        IEnumerable<string>? bcc = null, IEnumerable<string>? cc = null,
        string? attachmentFilePath = null, string? attachmentFileName = null,
        int attachedDownloadId = 0, IDictionary<string, string>? headers = null,
        CancellationToken ct = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName ?? string.Empty),
            Subject = subject ?? string.Empty,
            Body = body,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        message.To.Add(new MailAddress(toAddress, toName ?? string.Empty));

        if (!string.IsNullOrEmpty(replyToAddress))
        {
            message.ReplyToList.Add(new MailAddress(replyToAddress, replyToName ?? string.Empty));
        }

        //BCC
        if (bcc != null)
        {
            foreach (var address in bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
            {
                message.Bcc.Add(new MailAddress(address.Trim()));
            }
        }

        //CC
        if (cc != null)
        {
            foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
            {
                message.CC.Add(new MailAddress(address.Trim()));
            }
        }

        //headers
        if (headers != null)
        {
            foreach (var header in headers)
            {
                TryAddHeader(message, header.Key, header.Value);
            }
        }

        //send email
        using var smtpClient = await _smtpClientFactory.CreateAsync(emailAccount.Id, ct);
        //服务器拒收时 SendMailAsync 会抛 SmtpException，能执行到此处即表示已被服务器接收
        await smtpClient.SendMailAsync(message, ct);

        return true;
    }

    /// <summary>
    /// 追加自定义邮件头。<see cref="MailMessage.Headers"/> 不允许直接写入 From/To/Subject 等受限头，
    /// 这类头由上面各属性负责，写入失败时忽略并记录告警，避免影响邮件本体发送
    /// </summary>
    /// <param name="message">邮件消息</param>
    /// <param name="key">头名称</param>
    /// <param name="value">头值</param>
    private void TryAddHeader(MailMessage message, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        try
        {
            message.Headers.Add(key, value);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "自定义邮件头 {Header} 无法写入，已忽略", key);
        }
    }

    #endregion
}
