using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.Queued;
using Ape.Volo.IBusiness.Message.Email;
using Ape.Volo.IBusiness.Queued;
using Ape.Volo.SharedModel.Dto.Core.Queued;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Queued;
using Ape.Volo.ViewModel.Core.Queued;
using Microsoft.Extensions.Logging;

namespace Ape.Volo.Business.Queued;

/// <summary>
/// 邮件队列接口实现
/// </summary>
public class QueuedEmailService : BaseServices<QueuedEmail>, IQueuedEmailService
{
    #region 字段

    private readonly IEmailMessageTemplateService _emailMessageTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<QueuedEmailService> _logger;

    #endregion

    #region 构造函数

    /// <summary>
    /// 
    /// </summary>
    /// <param name="emailMessageTemplateService"></param>
    /// <param name="emailAccountService"></param>
    /// <param name="emailSender"></param>
    /// <param name="logger"></param>
    public QueuedEmailService(IEmailMessageTemplateService emailMessageTemplateService,
        IEmailAccountService emailAccountService, IEmailSender emailSender, ILogger<QueuedEmailService> logger)
    {
        _emailMessageTemplateService = emailMessageTemplateService;
        _emailAccountService = emailAccountService;
        _emailSender = emailSender;
        _logger = logger;
    }

    #endregion

    #region 基础方法

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto)
    {
        var emailAccount = await _emailAccountService.TableWhere(x => x.Id == createUpdateQueuedEmailDto.EmailAccountId)
            .SingleAsync();
        if (emailAccount.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateQueuedEmailDto,
                LanguageKeyConstants.EmailAccount,
                nameof(createUpdateQueuedEmailDto.Id)));
        }

        createUpdateQueuedEmailDto.From = emailAccount.Email;
        createUpdateQueuedEmailDto.FromName = emailAccount.DisplayName;
        var queuedEmail = App.Mapper.MapTo<QueuedEmail>(createUpdateQueuedEmailDto);
        var result = await AddAsync(queuedEmail);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="queuedEmail"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateTriesAsync(QueuedEmail queuedEmail)
    {
        var result = await UpdateAsync(queuedEmail);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateQueuedEmailDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateQueuedEmailDto createUpdateQueuedEmailDto)
    {
        if (!await TableWhere(x => x.Id == createUpdateQueuedEmailDto.Id).AnyAsync())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateQueuedEmailDto,
                LanguageKeyConstants.QueuedEmail,
                nameof(createUpdateQueuedEmailDto.Id)));
        }

        var emailAccount = await _emailAccountService.TableWhere(x => x.Id == createUpdateQueuedEmailDto.EmailAccountId)
            .SingleAsync();
        if (emailAccount.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateQueuedEmailDto,
                LanguageKeyConstants.EmailAccount,
                nameof(createUpdateQueuedEmailDto.EmailAccountId)));
        }

        createUpdateQueuedEmailDto.From = emailAccount.Email;
        createUpdateQueuedEmailDto.FromName = emailAccount.DisplayName;
        var queuedEmail = App.Mapper.MapTo<QueuedEmail>(createUpdateQueuedEmailDto);
        var result = await UpdateAsync(queuedEmail);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var emailAccounts = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (emailAccounts.Count < 1)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var result = await LogicDelete<QueuedEmail>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="queuedEmailQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<QueuedEmailVo>> QueryAsync(QueuedEmailQueryCriteria queuedEmailQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<QueuedEmail>
        {
            Pagination = pagination,
            ConditionalModels = queuedEmailQueryCriteria.ApplyQueryConditionalModel(),
        };
        return App.Mapper.MapTo<List<QueuedEmailVo>>(
            await TablePageAsync(queryOptions));
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 变更邮箱验证码
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="messageTemplateName"></param>
    /// <returns></returns>
    public async Task<OperateResult> ResetEmailCode(string emailAddress, string messageTemplateName)
    {
        var emailMessageTemplate =
            await _emailMessageTemplateService.TableWhere(x => x.Name == messageTemplateName).FirstAsync();
        if (emailMessageTemplate.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var emailAccount = await _emailAccountService.TableWhere(x => x.Id == emailMessageTemplate.EmailAccountId)
            .SingleAsync();
        if (emailAccount.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        //生成6位随机码
        var captcha = SixLaborsImageHelper.BuilEmailCaptcha(6);

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

        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.EmailCaptcha +
                                    queuedEmail.To.ToMd5String());
        var isTrue = await App.Cache.SetAsync(
            GlobalConstants.CachePrefix.EmailCaptcha + queuedEmail.To.ToMd5String16(), captcha,
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
                    await _emailAccountService.TableWhere(x => x.Id == queuedEmail.EmailAccountId).FirstAsync(),
                    queuedEmail.Subject,
                    queuedEmail.Body,
                    queuedEmail.From,
                    queuedEmail.FromName,
                    queuedEmail.To,
                    queuedEmail.ToName,
                    queuedEmail.ReplyTo,
                    queuedEmail.ReplyToName,
                    bcc,
                    cc);
                queuedEmail.IsSend = isTrue;
                if (isTrue)
                {
                    queuedEmail.SendTime = DateTime.Now;
                }
                // 如果开启redis并且开启消息队列功能 可以使用下面方式
                // await App.Cache.GetDatabase()
                //     .ListLeftPushAsync(MqTopicNameKey.MailboxQueue, queuedEmail.Id.ToString());
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
                    await AddAsync(queuedEmail);
                }
                catch
                {
                    // ignored
                }
            }
        }
        if (isTrue)
        {
            return OperateResult.Success("发送成功，验证码有效期5分钟");
        }
        return OperateResult.Result(isTrue);
    }

    /// <summary>
    /// 查询 发送邮件
    /// </summary>
    /// <param name="queuedEmailQueryCriteria"></param>
    /// <returns></returns>
    public Task<List<QueuedEmail>> QueryToSendMailAsync(QueuedEmailQueryCriteria queuedEmailQueryCriteria)
    {
        return TableWhere(queuedEmailQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
    }

    #endregion
}
