using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Log.Models;

/// <summary>
/// 日志查询参数
/// </summary>
public class LogQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 描述
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Description { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public string? Method { get; set; }

    /// <summary>
    /// 创建者账号
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? CreatorName { get; set; }
}
