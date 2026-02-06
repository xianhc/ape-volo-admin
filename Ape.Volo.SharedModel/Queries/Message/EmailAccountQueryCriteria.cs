using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.Message;

/// <summary>
/// 邮箱账户查询参数
/// </summary>
public class EmailAccountQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 邮箱
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Email { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string DisplayName { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string UserName { get; set; }
}
