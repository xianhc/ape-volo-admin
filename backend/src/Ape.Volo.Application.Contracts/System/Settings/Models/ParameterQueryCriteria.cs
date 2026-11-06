using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.Settings.Models;

/// <summary>
/// 全局设置查询参数
/// </summary>
public class ParameterQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 设置名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Name { get; set; }

    /// <summary>
    /// 数据值
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Value { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public bool? Enabled { get; set; }
}