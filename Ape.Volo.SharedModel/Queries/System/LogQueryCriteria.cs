using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.System;

/// <summary>
/// 日志查询参数
/// </summary>
public class LogQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 描述
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Description { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public string Method { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string CreateBy { get; set; }
}
