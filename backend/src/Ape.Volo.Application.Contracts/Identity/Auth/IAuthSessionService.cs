namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 登录会话服务，负责会话的创建、查询、轮换与撤销
/// </summary>
public interface IAuthSessionService
{
    /// <summary>
    /// 创建会话并维护用户撤销版本键
    /// </summary>
    /// <param name="session">会话对象</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>创建成功返回 true</returns>
    Task<bool> CreateAsync(AuthSession session, CancellationToken ct = default);

    /// <summary>
    /// 按会话标识查找仍然有效的会话
    /// </summary>
    /// <param name="sessionId">会话标识</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>有效会话；不存在、已过期或版本失效时返回 null</returns>
    Task<AuthSession?> GetActiveAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// 原子轮换刷新令牌哈希与访问令牌标识
    /// </summary>
    /// <param name="sessionId">会话标识</param>
    /// <param name="previousHash">原刷新令牌哈希</param>
    /// <param name="nextHash">新刷新令牌哈希</param>
    /// <param name="accessTokenId">新访问令牌标识</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>轮换成功返回 true；并发冲突或校验失败返回 false</returns>
    Task<bool> RotateAsync(string sessionId, string previousHash, string nextHash, string accessTokenId,
        CancellationToken ct = default);

    /// <summary>
    /// 撤销指定会话
    /// </summary>
    /// <param name="sessionId">会话标识</param>
    /// <param name="ct">取消令牌</param>
    Task RevokeAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// 撤销指定用户的全部会话
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <param name="ct">取消令牌</param>
    Task RevokeUserAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 列出全部在线会话
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>按创建时间倒序的会话集合</returns>
    Task<List<AuthSession>> ListActiveAsync(CancellationToken ct = default);
}
