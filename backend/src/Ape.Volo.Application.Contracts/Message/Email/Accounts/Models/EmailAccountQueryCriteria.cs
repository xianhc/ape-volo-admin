using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Message.Email.Accounts.Models;

/// <summary>
/// 邮箱账户查询参数
/// </summary>
public class EmailAccountQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 邮箱
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Email { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? UserName { get; set; }
}
