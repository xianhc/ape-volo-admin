namespace Ape.Volo.Shared.Model;

/// <summary>
/// 分页列表结果
/// </summary>
/// <typeparam name="T">返回集合中单条记录的类型</typeparam>
public class PagedResultDto<T> : ListResultDto<T>, IPagedResult<T>
{
    /// <summary>
    /// 初始化分页列表结果
    /// </summary>
    public PagedResultDto()
    {
    }

    /// <summary>
    /// 初始化分页列表结果
    /// </summary>
    /// <param name="totalCount">分页前的结果总数</param>
    /// <param name="items">当前页结果项</param>
    public PagedResultDto(int totalCount, IReadOnlyList<T> items)
        : base(items)
    {
        TotalCount = totalCount;
    }

    /// <summary>
    /// 分页前的结果总数
    /// </summary>
    public int TotalCount { get; set; }
}
