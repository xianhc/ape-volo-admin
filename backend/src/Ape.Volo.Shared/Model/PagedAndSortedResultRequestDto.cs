namespace Ape.Volo.Shared.Model;

/// <summary>
/// 分页和排序请求基类
/// </summary>
public class PagedAndSortedResultRequestDto : PagedResultRequestDto, IPagedAndSortedResultRequest
{
    /// <summary>
    /// 排序表达式，格式："字段名 asc|desc"
    /// </summary>
    public string? Sorting { get; set; }
}
