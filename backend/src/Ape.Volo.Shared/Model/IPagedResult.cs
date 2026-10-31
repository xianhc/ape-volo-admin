namespace Ape.Volo.Shared.Model;

/// <summary>
/// 组合列表和总数结果
/// </summary>
/// <typeparam name="T">返回集合中单条记录的类型</typeparam>
public interface IPagedResult<T> : IListResult<T>, IHasTotalCount
{
}
