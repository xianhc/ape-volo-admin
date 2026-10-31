using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Message.Email.Templates.Models;

/// <summary>
/// 邮箱模板查询参数
/// </summary>
public class EmailMessageTemplateQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Name { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public bool? Enabled { get; set; }
}
