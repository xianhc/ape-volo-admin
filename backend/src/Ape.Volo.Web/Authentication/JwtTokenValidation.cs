using System.Text;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.IdentityModel.Tokens;

namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 为访问和刷新流程提供一致的 JWT 真实性校验规则。
/// </summary>
public static class JwtTokenValidation
{
    /// <summary>
    /// 访问令牌用途标识
    /// </summary>
    public const string AccessTokenUse = "access";

    /// <summary>
    /// 刷新令牌用途标识
    /// </summary>
    public const string RefreshTokenUse = "refresh";

    /// <summary>
    /// 按配置创建令牌校验参数，用途不同则校验的 JWT typ 不同
    /// </summary>
    /// <param name="options">JWT 配置项</param>
    /// <param name="validateLifetime">是否校验有效期</param>
    /// <param name="tokenUse">令牌用途，默认访问令牌</param>
    /// <returns>令牌校验参数</returns>
    /// <exception cref="ArgumentNullException">配置项为 null</exception>
    /// <exception cref="InvalidOperationException">配置项不合法</exception>
    public static TokenValidationParameters CreateParameters(JwtAuthOptions options, bool validateLifetime,
        string tokenUse = AccessTokenUse)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecurityKey)),
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidTypes = [tokenUse == AccessTokenUse ? "at+jwt" : "rt+jwt"],
            NameClaimType = "name"
        };
    }

    /// <summary>
    /// 校验 JWT 配置是否完整有效，启动阶段快速失败
    /// </summary>
    /// <param name="options">JWT 配置项</param>
    /// <exception cref="InvalidOperationException">密钥、签发方、受众或有效期配置不合法</exception>
    public static void ValidateOptions(JwtAuthOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SecurityKey) ||
            Encoding.UTF8.GetByteCount(options.SecurityKey) < 32)
            throw new InvalidOperationException("必须配置 JwtAuth:SecurityKey，密钥长度至少为 32 字节。");
        if (string.IsNullOrWhiteSpace(options.Issuer) || string.IsNullOrWhiteSpace(options.Audience))
            throw new InvalidOperationException("必须配置 JwtAuth:Issuer 和 JwtAuth:Audience。");
        if (options.AccessTokenLifetimeMs <= 0 || options.RefreshTokenLifetimeMs <= 0 ||
            options.AccessTokenLifetimeMs > options.RefreshTokenLifetimeMs)
            throw new InvalidOperationException(
                "JWT 有效期必须为正数（单位：毫秒）：JwtAuth:AccessTokenLifetimeMs 不能超过 JwtAuth:RefreshTokenLifetimeMs。");
    }
}
