using Ape.Volo.Common.Attributes;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.System;

/// <summary>
/// 字典详情查询参数
/// </summary>
public class DictDetailQueryCriteria : IConditionalModel
{
    /// <summary>
    /// 字典ID
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public long DictId { get; set; }

    /// <summary>
    /// 标签名
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Label { get; set; }

    /// <summary>
    /// 数据值
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Value { get; set; }
}
