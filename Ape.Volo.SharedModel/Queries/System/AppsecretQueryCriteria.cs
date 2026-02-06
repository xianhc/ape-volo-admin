using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.System;

/// <summary>
/// 密钥查询参数
/// </summary>
public class AppsecretQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 应用Id
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public long AppId { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string AppName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Remark { get; set; }
}
