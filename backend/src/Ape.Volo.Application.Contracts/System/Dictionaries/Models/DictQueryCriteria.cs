using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.Dictionaries.Models;

/// <summary>
/// 字典查询参数
/// </summary>
public class DictQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 字典名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Name { get; set; }

    /// <summary>
    /// 字典描述
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Description { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public DictType? DictType { get; set; }
}
