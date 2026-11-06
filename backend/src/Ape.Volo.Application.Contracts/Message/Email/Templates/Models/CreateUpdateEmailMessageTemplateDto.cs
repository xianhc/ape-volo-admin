using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Message.Email.Templates.Models;

/// <summary>
/// 邮箱模板Dto
/// </summary>
public sealed record CreateUpdateEmailMessageTemplateDto
{
    /// <summary>
    /// 主键；新增时可省略，修改时指定目标记录
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// 模板名称
    /// </summary>
    [Display(Name = "模板名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 抄送邮箱地址
    /// </summary>
    public string? BccEmailAddresses { get; init; }

    /// <summary>
    /// 主题
    /// </summary>
    [Display(Name = "发送主题")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// 内容
    /// </summary>
    [Display(Name = "发送内容")]
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Body { get; init; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 邮箱账户标识符
    /// </summary>
    public long EmailAccountId { get; init; }
}
