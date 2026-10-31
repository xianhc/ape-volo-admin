namespace Ape.Volo.Shared.Model;

/// <summary>
/// 定义排序请求
/// </summary>
public interface ISortedResultRequest
{
    /// <summary>
    /// 排序表达式，格式："字段名 asc|desc"
    /// </summary>
    string? Sorting { get; set; }
}
