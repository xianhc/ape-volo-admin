using SqlSugar;

namespace Ape.Volo.SharedModel.Queries.Common;

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
        SortField = "id";
        OrderByType = OrderByType.Desc;
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
    /// 排序列
    /// </summary>
    public string SortField { get; set; }

    /// <summary>
    /// 排序方式
    /// </summary>
    public OrderByType OrderByType { get; set; }

    /// <summary>
    /// 总条数
    /// </summary>
    public int TotalElements { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }
}
