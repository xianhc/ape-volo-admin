namespace Ape.Volo.Shared.Model;

/// <summary>
/// 限制返回数量的请求基类
/// </summary>
public class LimitedResultRequestDto : ILimitedResultRequest
{
    /// <summary>
    /// 每页返回数量
    /// </summary>
    public int? PageSize { get; set; }
}
