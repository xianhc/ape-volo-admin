using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Permission.Menus.Models;

/// <summary>
/// 菜单查询参数
/// </summary>
public class MenuQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 菜单标题
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Title { get; set; }

    /// <summary>
    /// 父级ID
    /// 传值表示只查该父级下的子节点；不传表示不按父级过滤
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal, IsGreaterThanNumberDefault = false)]
    public long? ParentId { get; set; }

    /// <summary>
    /// 是否只查顶级菜单（父级为 null）
    /// 映射到 parent_id IS NULL
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.IsNull, FieldName = "ParentId")]
    public bool? IsRoot { get; set; }
}
