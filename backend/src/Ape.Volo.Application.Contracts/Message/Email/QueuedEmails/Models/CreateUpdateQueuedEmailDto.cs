using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Message.Email.QueuedEmails.Models;

/// <summary>
/// 邮件队列Dto
/// </summary>
public class CreateUpdateQueuedEmailDto : BaseEntityDto<long>
{
    /// <summary>
    /// 发件邮箱
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// 发件人名称
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// 收件邮箱
    /// </summary>
    [Display(Name = "收件邮箱")]
    [Required(ErrorMessage = "{0}required")]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// 收件人名称
    /// </summary>
    public string? ToName { get; set; }

    /// <summary>
    /// 回复邮箱
    /// </summary>
    public string? ReplyTo { get; set; }

    /// <summary>
    /// 回复人名称
    /// </summary>
    public string? ReplyToName { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    [Display(Name = "优先级")]
    [Range(0, 1, ErrorMessage = "{0}range{1}{2}")]
    public int Priority { get; set; }

    /// <summary>
    /// 抄送
    /// </summary>
    public string? Cc { get; set; }

    /// <summary>
    /// 密件抄送
    /// </summary>
    public string? Bcc { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}required")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Required(ErrorMessage = "{0}required")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 发送上限次数
    /// </summary>
    public int SentTries { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendTime { get; set; }

    /// <summary>
    /// 发件邮箱ID
    /// </summary>
    [Display(Name = "发件邮箱ID")]
    public long EmailAccountId { get; set; }
}
