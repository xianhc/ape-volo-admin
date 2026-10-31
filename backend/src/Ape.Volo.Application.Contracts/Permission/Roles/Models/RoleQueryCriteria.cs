using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Permission.Roles.Models;

/// <summary>
/// 角色查询参数
/// </summary>
public class RoleQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 角色名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Name { get; set; }

    /// <summary>
    /// 数据权限类型
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public int? DataScopeType { get; set; }
}
