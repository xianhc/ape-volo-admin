using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// JWT配置
/// </summary>
[OptionsSettings]
public class JwtAuthOptions
{
    /// <summary>
    /// 听众
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 发行者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 签名密钥
    /// </summary>
    public string SecurityKey { get; set; } = string.Empty;

    /// <summary>
    /// 访问令牌有效期（毫秒，默认 900000 即 15 分钟）。
    /// 实际签发时取「当前时间 + 本值」与会话绝对截止时间中的较小者。
    /// </summary>
    public long AccessTokenLifetimeMs { get; set; } = 900_000;

    /// <summary>
    /// 刷新令牌有效期（毫秒，默认 604800000 即 7 天）。
    /// 该值决定会话的绝对截止时间：自登录起算，期间刷新令牌只轮换、不会延长该截止时间，到期必须重新登录。
    /// 与登录响应里的 TokenVo.RefreshTokenExpires（Unix 毫秒时间戳）单位相同但语义不同：本项是时长，后者是时刻。
    /// </summary>
    public long RefreshTokenLifetimeMs { get; set; } = 604_800_000;

    /// <summary>
    /// 登录路径
    /// </summary>
    public string LoginPath { get; set; } = string.Empty;
}
