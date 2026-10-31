using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Operations.Models;

/// <summary>
/// 任务调度查询参数
/// </summary>
public class ScheduledJobQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 任务名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? JobName { get; set; }


    /// <summary>
    /// 任务组
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? JobGroup { get; set; }


    /// <summary>
    /// 任务描述
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public bool? Enabled { get; set; }
}
