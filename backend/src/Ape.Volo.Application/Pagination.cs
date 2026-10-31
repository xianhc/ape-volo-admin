using Ape.Volo.Shared.Model;
using SqlSugar;
using System.Diagnostics.CodeAnalysis;

namespace Ape.Volo.Application;

/// <summary>
/// 分页
/// </summary>
public class Pagination
{
    /// <summary>
    /// 
    /// </summary>
    public Pagination()
    {
        PageIndex = 1;
        PageSize = 10;
    }

    /// <summary>
    /// 由分页请求创建分页对象
    /// 注：未传值时使用默认值（第 1 页、每页 10 条）
    /// </summary>
    /// <param name="request">分页请求</param>
    /// <returns></returns>
    public static Pagination From(IPagedAndSortedResultRequest request)
    {
        var pagination = new Pagination();
        if (request == null)
        {
            return pagination;
        }

        if (request.PageIndex.HasValue)
        {
            pagination.PageIndex = request.PageIndex.Value;
        }

        if (request.PageSize.HasValue)
        {
            pagination.PageSize = request.PageSize.Value;
        }

        pagination.Sorting = request.Sorting;
        return pagination;
    }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页行数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 排序字符串，格式："字段名 asc|desc"
    /// </summary>
    public string? Sorting { get; set; } = string.Empty;

    /// <summary>
    /// 解析排序字符串为排序字段与方向
    /// </summary>
    /// <param name="sorting">排序字符串，格式："字段名 asc|desc"，方向缺省按 asc</param>
    /// <param name="sortField">解析出的排序字段</param>
    /// <param name="orderByType">解析出的排序方向</param>
    /// <returns>解析成功且字段非空时为 true</returns>
    public static bool TryParseSorting(string? sorting, [NotNullWhen(true)] out string? sortField, out OrderByType orderByType)
    {
        sortField = null;
        orderByType = OrderByType.Asc;
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return false;
        }

        var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
        {
            return false;
        }

        sortField = parts[0].Trim();
        if (parts.Length > 1 && parts[1].Trim().Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            orderByType = OrderByType.Desc;
        }

        return true;
    }

    /// <summary>
    /// 总条数
    /// </summary>
    public int TotalElements { get; set; }
}
