namespace Ape.Volo.Application.Contracts.Message.Email.Templates.Models;

/// <summary>
/// 邮箱模板Vo
/// </summary>
public class EmailMessageTemplateVo : BaseEntityDto<long>
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 抄送邮箱地址
    /// </summary>
    public string? BccEmailAddresses { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 发送邮箱账户
    /// </summary>
    public long EmailAccountId { get; set; }
}
