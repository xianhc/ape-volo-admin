namespace Ape.Volo.Shared.Model;

/// <summary>
/// 分页请求基类
/// </summary>
public class PagedResultRequestDto : LimitedResultRequestDto, IPagedResultRequest
{
    /// <summary>
    /// 当前页码
    /// </summary>
    public int? PageIndex { get; set; }
}
