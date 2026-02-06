using System.ComponentModel.DataAnnotations;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.Message.Email;

namespace Ape.Volo.SharedModel.Dto.Core.Message.Email;

/// <summary>
/// 邮箱模板Dto
/// </summary>
[AutoMapping(typeof(EmailMessageTemplate), typeof(CreateUpdateEmailMessageTemplateDto))]
public class CreateUpdateEmailMessageTemplateDto : BaseEntityDto<long>
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Display(Name = "EmailTemplate.Name")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; }

    /// <summary>
    /// 抄送邮箱地址
    /// </summary>
    public string BccEmailAddresses { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    [Display(Name = "EmailTemplate.Subject")]
    [Required(ErrorMessage = "{0}required")]
    public string Subject { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "EmailTemplate.Body")]
    [Required(ErrorMessage = "{0}required")]
    public string Body { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 邮箱账户标识符
    /// </summary>
    public long EmailAccountId { get; set; }
}
