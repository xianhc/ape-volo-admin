using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Log.Models;

/// <summary>
/// 任务调度日志查询参数
/// </summary>
public class ScheduledJobLogQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 任务ID
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal, FieldName = "ScheduledJobId")]
    public long? ScheduledJobId { get; set; }

    /// <summary>
    /// 是否执行成功
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public bool? IsSuccess { get; set; }
}
