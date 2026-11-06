namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 仅存于当前系统缓存中的登录会话，不映射数据库表。
/// </summary>
public class AuthSession
{
    /// <summary>
    /// 会话标识，作为缓存键后缀
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 会话所属用户标识
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户撤销版本号，与缓存中的版本不一致时会话即失效
    /// </summary>
    public string UserVersion { get; set; } = string.Empty;

    /// <summary>
    /// 当前有效访问令牌的标识，刷新后旧令牌随之失效
    /// </summary>
    public string AccessTokenId { get; set; } = string.Empty;

    /// <summary>
    /// 当前刷新令牌的哈希值，用于校验与一次性消费
    /// </summary>
    public string RefreshTokenHash { get; set; } = string.Empty;

    /// <summary>
    /// 登录用户信息的序列化结果，供刷新令牌时还原上下文
    /// </summary>
    public string LoginInfoJson { get; set; } = string.Empty;

    /// <summary>
    /// 会话创建时间（UTC）
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// 会话过期时间（UTC）
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }
}
