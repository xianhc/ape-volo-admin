using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.System;

/// <summary>
/// 字典查询参数
/// </summary>
public class DictQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 字典名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Name { get; set; }

    /// <summary>
    /// 字典描述
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Description { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public DictType? DictType { get; set; }
}
