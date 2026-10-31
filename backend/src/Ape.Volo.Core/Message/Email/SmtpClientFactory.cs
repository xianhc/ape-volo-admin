using System.Net;
using Ape.Volo.Application.Contracts.SystemManagement.Settings;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Message.Email;

namespace Ape.Volo.Core.Message.Email;

/// <summary>
/// SMTP 客户端工厂（基于框架内置 <see cref="SmtpClient"/>，不依赖 MailKit）
/// </summary>
public class SmtpClientFactory : ISmtpClientFactory
{
    #region Fields

    private readonly ISugarRepository<EmailAccount> _emailAccountRepository;
    private readonly ISettingService _settingService;
    private readonly IOptions<EmailOptions> _emailOptions;
    private readonly ILogger<SmtpClientFactory> _logger;

    /// <summary>
    /// 证书校验配置告警只记录一次，避免每封邮件都刷日志
    /// </summary>
    private int _certificateWarningLogged;

    #endregion

    #region Ctor

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="emailAccountRepository">邮箱账户仓储（SMTP 认证需要密码，契约 VO 不暴露该字段）</param>
    /// <param name="settingService">全局设置服务</param>
    /// <param name="emailOptions">邮件配置</param>
    /// <param name="logger">当前宿主日志</param>
    public SmtpClientFactory(ISugarRepository<EmailAccount> emailAccountRepository, ISettingService settingService,
        IOptions<EmailOptions> emailOptions, ILogger<SmtpClientFactory> logger)
    {
        _emailAccountRepository = emailAccountRepository;
        _settingService = settingService;
        _emailOptions = emailOptions;
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<SmtpClient> CreateAsync(long? emailAccountId = null, CancellationToken ct = default)
    {
        var accountId = emailAccountId
                        ?? await _settingService.GetSettingValue<long>("DefaultEmailAccountId", ct);
        var emailAccount = await _emailAccountRepository.SugarClient.Queryable<EmailAccount>()
                               .FirstAsync(x => x.Id == accountId, ct)
                           ?? throw new Exception("Email account could not be loaded");

        var client = new SmtpClient(emailAccount.Host, emailAccount.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = emailAccount.EnableSsl,
            UseDefaultCredentials = emailAccount.UseDefaultCredentials
        };

        if (!emailAccount.UseDefaultCredentials && !string.IsNullOrWhiteSpace(emailAccount.UserName))
        {
            client.Credentials = new NetworkCredential(emailAccount.UserName, emailAccount.Password ?? string.Empty);
        }

        WarnIfCertificateValidationDisabled();

        return client;
    }

    /// <summary>
    /// <see cref="SmtpClient"/> 不支持自定义证书校验回调，始终校验服务端证书。
    /// 配置中若关闭了校验，这里只给出一次告警，避免调用方误以为已经生效
    /// </summary>
    private void WarnIfCertificateValidationDisabled()
    {
        var options = _emailOptions.Value;
        if (options.ValidateServerCertificate)
        {
            return;
        }

        if (Interlocked.Exchange(ref _certificateWarningLogged, 1) == 0)
        {
            _logger.LogWarning(
                "Email:ValidateServerCertificate=false 已不再生效：System.Net.Mail 始终校验 SMTP 服务端证书。" +
                "如需连接自签证书服务器，请把该证书安装到系统受信任根证书存储中");
        }
    }

    #endregion
}
