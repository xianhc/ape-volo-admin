using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Organization.Departments.Models;

/// <summary>
/// 部门查询参数
/// </summary>
public class DepartmentQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 部门名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Name { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public bool? Enabled { get; set; }

    /// <summary>
    /// 父级ID
    /// 传值表示只查该父级下的子节点；不传表示不按父级过滤
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal, IsGreaterThanNumberDefault = false)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 是否只查顶级部门（父级为 null）
    /// 映射到 parent_id IS NULL
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.IsNull, FieldName = "ParentId")]
    public bool? IsRoot { get; set; }
}
