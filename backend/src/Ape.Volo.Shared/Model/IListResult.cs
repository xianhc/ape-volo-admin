namespace Ape.Volo.Shared.Model;

/// <summary>
/// 定义列表结果
/// </summary>
/// <typeparam name="T">返回集合中单条记录的类型</typeparam>
public interface IListResult<T>
{
    /// <summary>
    /// 结果项
    /// </summary>
    IReadOnlyList<T> Items { get; set; }
}
