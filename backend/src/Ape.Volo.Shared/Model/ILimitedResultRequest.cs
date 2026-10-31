namespace Ape.Volo.Shared.Model;

/// <summary>
/// 定义限制返回数量的请求
/// </summary>
public interface ILimitedResultRequest
{
    /// <summary>
    /// 每页返回数量
    /// </summary>
    int? PageSize { get; set; }
}
