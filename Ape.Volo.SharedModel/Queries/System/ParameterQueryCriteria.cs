using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.System;

/// <summary>
/// 全局设置查询参数
/// </summary>
public class ParameterQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 设置名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Name { get; set; }

    /// <summary>
    /// 数据值
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Value { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public bool? Enabled { get; set; }
}