namespace Ape.Volo.Shared.Model;

/// <summary>
/// 定义从一开始计数的分页请求
/// </summary>
public interface IPagedResultRequest : ILimitedResultRequest
{
    /// <summary>
    /// 当前页码
    /// </summary>
    int? PageIndex { get; set; }
}
