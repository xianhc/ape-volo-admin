using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.AppSecrets.Models;

/// <summary>
/// 密钥查询参数
/// </summary>
public class AppSecretQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 应用Id
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Equal)]
    public long? AppId { get; set; }

    /// <summary>
    /// 应用名称
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? AppName { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Remark { get; set; }
}
