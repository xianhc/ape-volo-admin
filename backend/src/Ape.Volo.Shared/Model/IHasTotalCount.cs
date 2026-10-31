namespace Ape.Volo.Shared.Model;

/// <summary>
/// 定义总数结果
/// </summary>
public interface IHasTotalCount
{
    /// <summary>
    /// 分页前的结果总数
    /// </summary>
    int TotalCount { get; set; }
}
