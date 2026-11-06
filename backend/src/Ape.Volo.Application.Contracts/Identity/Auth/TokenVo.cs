

namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 登录成功后返回的令牌对，包含访问令牌、刷新令牌及各自的有效期。
/// </summary>
public class TokenVo
{
    /// <summary>
    /// 本次登录的稳定会话标识，不可用于认证。
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// 授权token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌过期时刻（Unix 毫秒时间戳），非时长
    /// </summary>
    public long Expires { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// 独立刷新令牌；每次刷新后必须保存新值，不能作为 API 访问凭证
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 会话绝对截止时刻（Unix 毫秒时间戳），刷新不会延长。
    /// 与配置项 JwtAuth:RefreshTokenLifetimeMs（毫秒时长）单位相同但语义不同：本项是时刻，配置项是时长。
    /// </summary>
    public long RefreshTokenExpires { get; set; }
}
