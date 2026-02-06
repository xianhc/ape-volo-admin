using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.Permission;

/// <summary>
/// 角色查询参数
/// </summary>
public class RoleQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string Name { get; set; }

    /// <summary>
    /// 数据权限类型
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public int DataScopeType { get; set; }
}
