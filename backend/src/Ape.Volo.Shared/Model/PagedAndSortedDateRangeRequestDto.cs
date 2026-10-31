using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Shared.Model;

/// <summary>
/// 带创建时间范围的分页和排序请求基类
/// </summary>
public class PagedAndSortedDateRangeRequestDto : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 创建时间范围的开始时间，包含边界
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.GreaterThanOrEqual, FieldName = "CreationTime")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 创建时间范围的结束时间，包含边界
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.LessThanOrEqual, FieldName = "CreationTime")]
    public DateTime? EndTime { get; set; }
}
