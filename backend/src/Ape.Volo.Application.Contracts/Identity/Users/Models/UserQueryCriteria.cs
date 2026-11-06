using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Identity.Users.Models;

/// <summary>
/// 用户查询参数
/// </summary>
public class UserQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public long? Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? UserName { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? NickName { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public bool? Enabled { get; set; }

    /// <summary>
    /// 部门ID集合 用于查询
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.In, FieldName = "DeptId")]
    public string? DepartmentIdArray { get; set; }
}
