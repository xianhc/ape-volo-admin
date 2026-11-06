using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.FileRecords.Models;

/// <summary>
/// 文件记录查询参数
/// </summary>
public class FileRecordQueryCriteria : PagedAndSortedDateRangeRequestDto, IPagedQuery
{
    /// <summary>
    /// 关键字
    /// </summary>
    [QueryCondition(ConditionType = QueryConditionType.Like)]
    public string? Description { get; set; }
}
