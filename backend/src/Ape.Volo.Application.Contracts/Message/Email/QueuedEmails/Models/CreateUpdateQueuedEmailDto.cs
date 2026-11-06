using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Message.Email.QueuedEmails.Models;

/// <summary>
/// 邮件队列Dto
/// </summary>
public sealed record CreateUpdateQueuedEmailDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 发件邮箱
    /// </summary>
    public string From { get; init; } = string.Empty;

    /// <summary>
    /// 发件人名称
    /// </summary>
    public string? FromName { get; init; }

    /// <summary>
    /// 收件邮箱
    /// </summary>
    [Display(Name = "收件邮箱")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string To { get; init; } = string.Empty;

    /// <summary>
    /// 收件人名称
    /// </summary>
    public string? ToName { get; init; }

    /// <summary>
    /// 回复邮箱
    /// </summary>
    public string? ReplyTo { get; init; }

    /// <summary>
    /// 回复人名称
    /// </summary>
    public string? ReplyToName { get; init; }

    /// <summary>
    /// 优先级
    /// </summary>
    [Display(Name = "优先级")]
    [Range(0, 1, ErrorMessage = "{0}必须在{1}到{2}之间。")]
    public int Priority { get; init; }

    /// <summary>
    /// 抄送
    /// </summary>
    public string? Cc { get; init; }

    /// <summary>
    /// 密件抄送
    /// </summary>
    public string? Bcc { get; init; }

    /// <summary>
    /// 标题
    /// </summary>
    [Display(Name = "标题")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "内容")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Body { get; init; } = string.Empty;

    /// <summary>
    /// 发送上限次数
    /// </summary>
    public int SentTries { get; init; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime? SendTime { get; init; }

    /// <summary>
    /// 发件邮箱ID
    /// </summary>
    [Display(Name = "发件邮箱ID")]
    public long EmailAccountId { get; init; }
}
