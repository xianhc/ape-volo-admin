using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.System;

/// <summary>
/// 任务调度查询参数
/// </summary>
public class QuartzNetQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 任务名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string TaskName { get; set; }


    /// <summary>
    /// 任务组
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string TaskGroup { get; set; }


    /// <summary>
    /// 任务描述
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public bool? Enabled { get; set; }
}
