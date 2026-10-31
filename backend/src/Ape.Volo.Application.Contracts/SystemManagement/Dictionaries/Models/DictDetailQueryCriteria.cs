using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.SystemManagement.Dictionaries.Models;

/// <summary>
/// 字典详情查询参数
/// </summary>
public class DictDetailQueryCriteria : PagedAndSortedResultRequestDto, IPagedQuery
{
    /// <summary>
    /// 字典ID
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public long? DictId { get; set; }

    /// <summary>
    /// 标签名
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Label { get; set; }

    /// <summary>
    /// 数据值
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Value { get; set; }
}
