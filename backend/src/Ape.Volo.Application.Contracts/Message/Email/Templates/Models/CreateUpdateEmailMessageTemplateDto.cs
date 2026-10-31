using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Message.Email.Templates.Models;

/// <summary>
/// 邮箱模板Dto
/// </summary>
public class CreateUpdateEmailMessageTemplateDto : BaseEntityDto<long>
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Display(Name = "模板名称")]
    [Required(ErrorMessage = "{0}required")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 抄送邮箱地址
    /// </summary>
    public string? BccEmailAddresses { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    [Display(Name = "发送主题")]
    [Required(ErrorMessage = "{0}required")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "发送内容")]
    [Required(ErrorMessage = "{0}required")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 邮箱账户标识符
    /// </summary>
    public long EmailAccountId { get; set; }
}
