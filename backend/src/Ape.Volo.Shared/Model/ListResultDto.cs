namespace Ape.Volo.Shared.Model;

/// <summary>
/// 列表结果
/// </summary>
/// <typeparam name="T">返回集合中单条记录的类型</typeparam>
public class ListResultDto<T> : IListResult<T>
{
    /// <summary>
    /// 初始化列表结果
    /// </summary>
    public ListResultDto()
        : this([])
    {
    }

    /// <summary>
    /// 初始化列表结果
    /// </summary>
    /// <param name="items">结果项</param>
    public ListResultDto(IReadOnlyList<T> items)
    {
        Items = items;
    }

    /// <summary>
    /// 结果项
    /// </summary>
    public IReadOnlyList<T> Items { get; set; }
}
