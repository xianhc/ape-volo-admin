using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.Permission;

/// <summary>
/// 用户查询参数
/// </summary>
public class UserQueryCriteria : DateRange, IConditionalModel
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public long Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string UserName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Like)]
    public string NickName { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.Equal)]
    public bool? Enabled { get; set; }

    /// <summary>
    /// 部门ID集合 用于查询
    /// </summary>
    [QueryCondition(ConditionType = ConditionalType.In, FieldName = "DeptId")]
    public string DepartmentIdArray { get; set; }
}
