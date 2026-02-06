using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.Message;

/// <summary>
/// 邮箱模板查询参数
/// </summary>
public class EmailMessageTemplateQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Name { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public bool? Enabled { get; set; }
}
