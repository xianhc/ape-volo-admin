using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Message.Email.QueuedEmails.Models;
using Ape.Volo.Application.Contracts.Message.Email;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Message.Email.QueuedEmails;
using Ape.Volo.Entity.Message.Email;
using Microsoft.Extensions.Logging;

namespace Ape.Volo.Application.Message.Email.QueuedEmails;

/// <summary>
/// 邮件队列接口实现
/// </summary>
public class QueuedEmailService : BaseService<QueuedEmail>, IQueuedEmailService
{
    private const int DispatchBatchSize = 100;
    private const int MaxTries = 3;

    #region 字段

    private readonly IEmailMessageTemplateService _emailMessageTemplateService;
    private readonly IEmailAccountLookupService _emailAccountService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<QueuedEmailService> _logger;
    private readonly ICache _cache;

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="emailMessageTemplateService">邮件模板服务</param>
    /// <param name="emailAccountService">邮件账户服务</param>
    /// <param name="emailSender">邮件发送器</param>
    /// <param name="logger">日志</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="cache">缓存</param>
    public QueuedEmailService(ISugarRepository<QueuedEmail> sugarRepository,
        IEmailMessageTemplateService emailMessageTemplateService,
        IEmailAccountLookupService emailAccountService, IEmailSender emailSender, ILogger<QueuedEmailService> logger,
        IMapper mapper, ICache cache)
        : base(sugarRepository, mapper)
    {
        _emailMessageTemplateService = emailMessageTemplateService;
        _emailAccountService = emailAccountService;
        _emailSender = emailSender;
        _logger = logger;
        _cache = cache;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.QueuedEmails.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto, CancellationToken ct = default)
    {
        var emailAccount = await _emailAccountService.GetByIdAsync(createUpdateQueuedEmailDto.EmailAccountId, ct);
        if (emailAccount == null)
        {
            return NotFound<EmailAccount>(createUpdateQueuedEmailDto.EmailAccountId);
        }

        var queuedEmail = Mapper.MapTo<QueuedEmail>(createUpdateQueuedEmailDto);
        queuedEmail.From = emailAccount.Email;
        queuedEmail.FromName = emailAccount.DisplayName;
        var result = await AddAsync(queuedEmail, ct: ct);
        return OperateResult.Result(result);
    }

    private async Task UpdateTriesAsync(QueuedEmailVo queuedEmailVo, CancellationToken ct)
    {
        var queuedEmail = Mapper.MapTo<QueuedEmail>(queuedEmailVo);
        await UpdateAsync(queuedEmail, ct: ct);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.QueuedEmails.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto, CancellationToken ct = default)
    {
        if (!await TableWhere(x => x.Id == createUpdateQueuedEmailDto.Id).AnyAsync(ct))
        {
            return NotFound(createUpdateQueuedEmailDto.Id);
        }

        var emailAccount = await _emailAccountService.GetByIdAsync(createUpdateQueuedEmailDto.EmailAccountId, ct);
        if (emailAccount == null)
        {
            return NotFound<EmailAccount>(createUpdateQueuedEmailDto.EmailAccountId);
        }

        var queuedEmail = Mapper.MapTo<QueuedEmail>(createUpdateQueuedEmailDto);
        queuedEmail.From = emailAccount.Email;
        queuedEmail.FromName = emailAccount.DisplayName;
        var result = await UpdateAsync(queuedEmail, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.QueuedEmails.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var emailAccounts = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (emailAccounts.Count < 1)
        {
            return NotFound();
        }

        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.QueuedEmails.List)]
    public Task<PagedResultDto<QueuedEmailVo>> GetListAsync(QueuedEmailQueryCriteria queuedEmailQueryCriteria, CancellationToken ct = default)
        => PageAsync<QueuedEmailVo>(queuedEmailQueryCriteria, ct: ct);

    #endregion

    #region 扩展方法

    /// <inheritdoc />
    public async Task<OperateResult> ResetEmailCode(string emailAddress, string messageTemplateName, CancellationToken ct = default)
    {
        var emailMessageTemplate =
            await _emailMessageTemplateService.GetByNameAsync(messageTemplateName, ct);
        if (emailMessageTemplate == null)
        {
            return NotFound<EmailMessageTemplate>(messageTemplateName, nameof(EmailMessageTemplate.Name));
        }

        var emailAccount = await _emailAccountService.GetByIdAsync(emailMessageTemplate.EmailAccountId, ct);
        if (emailAccount == null)
        {
            return NotFound<EmailAccount>(emailMessageTemplate.EmailAccountId);
        }

        // 同一收件人 60 秒内只允许发送一次，避免被用来轰炸任意邮箱
        var cooldownKey = EmailCooldownKey(emailAddress);
        if (await _cache.GetAsync<int>(cooldownKey) > 0)
        {
            return OperateResult.Error("验证码发送过于频繁，请稍后再试");
        }

        //生成6位随机码
        var captcha = CaptchaHelper.BuildEmailCaptcha(6);

        QueuedEmail queuedEmail = new QueuedEmail();
        queuedEmail.From = emailAccount.Email;
        queuedEmail.FromName = emailAccount.DisplayName;
        queuedEmail.To = emailAddress;
        queuedEmail.Priority = QueuedEmailPriority.High;
        queuedEmail.Bcc = emailMessageTemplate.BccEmailAddresses;
        queuedEmail.Subject = emailMessageTemplate.Subject;
        queuedEmail.Body = emailMessageTemplate.Body.Replace("%captcha%", captcha);
        queuedEmail.SentTries = 1;
        queuedEmail.EmailAccountId = emailAccount.Id;

        var isTrue = await _cache.SetAsync(
            GlobalConstants.CachePrefix.EmailCaptcha + queuedEmail.To, captcha,
            TimeSpan.FromMinutes(5), null);

        if (isTrue)
        {
            var bcc = string.IsNullOrWhiteSpace(queuedEmail.Bcc)
                ? null
                : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var cc = string.IsNullOrWhiteSpace(queuedEmail.Cc)
                ? null
                : queuedEmail.Cc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                isTrue = await _emailSender.SendEmailAsync(
                    emailAccount,
                    queuedEmail.Subject ?? string.Empty,
                    queuedEmail.Body,
                    queuedEmail.From,
                    queuedEmail.FromName,
                    queuedEmail.To,
                    queuedEmail.ToName!,
                    queuedEmail.ReplyTo,
                    queuedEmail.ReplyToName,
                    bcc,
                    cc,
                    ct: ct);
                queuedEmail.IsSend = isTrue;
                if (isTrue)
                {
                    queuedEmail.SendTime = DateTime.Now;
                    await _cache.SetAsync(cooldownKey, 1, TimeSpan.FromSeconds(60), null);
                }
            }
            catch (Exception exc)
            {
                _logger.LogError($"Error sending e-mail. {exc.Message}");
                isTrue = false;
            }
            finally
            {
                try
                {
                    await AddAsync(queuedEmail, ct: ct);
                }
                catch (Exception exception)
                {
                    // 回写队列状态失败不应吞掉：发送已成功但记录未更新会导致重复发送
                    _logger.LogWarning(exception, "回写邮件队列状态失败，邮件 Id：{EmailId}", queuedEmail.Id);
                }
            }
        }
        if (isTrue)
        {
            return OperateResult.Success("发送成功，验证码有效期5分钟");
        }
        return OperateResult.Result(isTrue);
    }

    private async Task<List<QueuedEmailVo>> QueryToSendMailAsync(
        QueuedEmailQueryCriteria queuedEmailQueryCriteria, CancellationToken ct)
    {
        var queuedEmails = await TableWhere(queuedEmailQueryCriteria.ApplyQueryConditionalModel())
            .OrderBy(x => x.CreationTime)
            .Take(DispatchBatchSize)
            .ToListAsync(ct);
        return Mapper.MapTo<List<QueuedEmailVo>>(queuedEmails);
    }

    /// <inheritdoc />
    public async Task SendPendingAsync(CancellationToken ct = default)
    {
        var queuedEmails = await QueryToSendMailAsync(
            new QueuedEmailQueryCriteria { MaxTries = MaxTries, IsSend = false }, ct);
        var accountIds = queuedEmails.Select(x => x.EmailAccountId).Distinct().ToArray();
        var emailAccounts = await _emailAccountService.GetByIdsAsync(accountIds, ct);

        foreach (var queuedEmail in queuedEmails)
        {
            var bcc = string.IsNullOrWhiteSpace(queuedEmail.Bcc)
                ? null
                : queuedEmail.Bcc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var cc = string.IsNullOrWhiteSpace(queuedEmail.Cc)
                ? null
                : queuedEmail.Cc.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                if (!emailAccounts.TryGetValue(queuedEmail.EmailAccountId, out var emailAccount))
                {
                    _logger.LogWarning("邮件账户 {AccountId} 不存在，跳过队列邮件 {QueuedEmailId}",
                        queuedEmail.EmailAccountId, queuedEmail.Id);
                    continue;
                }

                queuedEmail.IsSend = await _emailSender.SendEmailAsync(
                    emailAccount,
                    queuedEmail.Subject ?? string.Empty,
                    queuedEmail.Body,
                    queuedEmail.From,
                    queuedEmail.FromName ?? string.Empty,
                    queuedEmail.To,
                    queuedEmail.ToName ?? string.Empty,
                    queuedEmail.ReplyTo,
                    queuedEmail.ReplyToName,
                    bcc,
                    cc,
                    ct: ct);

                if (queuedEmail.IsSend == true)
                {
                    queuedEmail.SendTime = DateTime.Now;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "发送队列邮件 {QueuedEmailId} 失败", queuedEmail.Id);
            }
            finally
            {
                queuedEmail.SentTries += 1;
                queuedEmail.LastModificationTime = DateTime.Now;
                await UpdateTriesAsync(queuedEmail, ct);
            }
        }
    }

    /// <summary>
    /// 邮件验证码的发送冷却键：按收件人地址隔离。
    /// </summary>
    /// <param name="emailAddress">收件人地址</param>
    /// <returns>缓存键</returns>
    private static string EmailCooldownKey(string emailAddress) =>
        GlobalConstants.CachePrefix.EmailCaptcha + "cooldown:" + emailAddress;

    #endregion
}
