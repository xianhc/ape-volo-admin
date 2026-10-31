using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Message.Email.QueuedEmails.Models;

/// <summary>
/// 队列邮件查询参数
/// </summary>
public class QueuedEmailQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 队列ID
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public long? Id { get; set; }

    /// <summary>
    /// 最大执行次数
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.LessThan, FieldName = "SentTries")]
    public int? MaxTries { get; set; }

    /// <summary>
    /// 发送方
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public long? EmailAccountId { get; set; }

    /// <summary>
    /// 接收方
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like, FieldNameItems = ["To", "ToName"])]
    public string? To { get; set; }

    /// <summary>
    /// 是否已发送
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public bool? IsSend { get; set; }
}
