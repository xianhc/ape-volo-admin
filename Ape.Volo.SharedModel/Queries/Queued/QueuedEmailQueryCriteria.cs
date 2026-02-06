using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.Queued;

/// <summary>
/// 队列邮件查询参数
/// </summary>
public class QueuedEmailQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 队列ID
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public long Id { get; set; }

    /// <summary>
    /// 最大执行次数
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.LessThan, FieldName = "SentTries")]
    public int MaxTries { get; set; }

    /// <summary>
    /// 发送方
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public long EmailAccountId { get; set; }

    /// <summary>
    /// 接收方
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like, FieldNameItems = ["To", "ToName"])]
    public string To { get; set; }

    /// <summary>
    /// 是否已发送
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public bool? IsSend { get; set; }
}
