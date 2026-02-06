using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.Message.Email;

namespace Ape.Volo.ViewModel.Core.Message.Email;

/// <summary>
/// 邮箱模板Vo
/// </summary>
[AutoMapping(typeof(EmailMessageTemplate), typeof(EmailMessageTemplateVo))]
public class EmailMessageTemplateVo : BaseEntityDto<long>
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 抄送邮箱地址
    /// </summary>
    public string BccEmailAddresses { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 发送邮箱账户
    /// </summary>
    public long EmailAccountId { get; set; }
}
