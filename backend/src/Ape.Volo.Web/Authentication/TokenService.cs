using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ape.Volo.Shared.WebApp;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 签发、轮换和撤销统一经过服务端会话；刷新凭证不能用来访问 API。
/// </summary>
public class TokenService(IOptions<JwtAuthOptions> options, IAuthSessionService sessions, TimeProvider clock)
    : ITokenService, ITokenIssuer
{
    private readonly JwtAuthOptions _options = options.Value;
    private DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    /// <summary>
    /// 应用层只依赖签发端口，JWT 细节不向上暴露。
    /// </summary>
    /// <param name="loginUserInfo">登录用户信息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>令牌对；会话创建失败时返回 null</returns>
    Task<TokenVo?> ITokenIssuer.IssueAsync(LoginUserInfo loginUserInfo, CancellationToken ct) =>
        IssueTokenAsync(loginUserInfo, ct);

    /// <inheritdoc />
    public async Task<TokenVo?> IssueTokenAsync(LoginUserInfo loginUserInfo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(loginUserInfo);
        var now = UtcNow;
        var session = new AuthSession
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = loginUserInfo.UserId,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMilliseconds(_options.RefreshTokenLifetimeMs),
            AccessTokenId = Guid.NewGuid().ToString("N")
        };
        // 在线列表只暴露会话标识，不保存或回传可用于认证的原始令牌。
        loginUserInfo.AccessToken = session.Id;
        session.LoginInfoJson = JsonConvert.SerializeObject(loginUserInfo);
        var pair = CreatePair(session, loginUserInfo);
        session.RefreshTokenHash = Hash(pair.RefreshToken);
        return await sessions.CreateAsync(session, ct) ? pair : null;
    }

    /// <inheritdoc />
    public async Task<TokenVo?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var principal = ReadToken(refreshToken, JwtTokenValidation.RefreshTokenUse, true);
        if (!TryGetIdentity(principal, JwtTokenValidation.RefreshTokenUse, out var userId, out var sessionId,
                out _)) return null;

        var session = await sessions.FindActiveAsync(sessionId, ct);
        if (session == null || session.UserId != userId) return null;

        var previousHash = Hash(refreshToken);
        if (!CryptographicOperations.FixedTimeEquals(Convert.FromHexString(session.RefreshTokenHash),
                Convert.FromHexString(previousHash)))
        {
            // 签名正确但已消费的刷新令牌再次出现：撤销整个会话。
            await sessions.RevokeAsync(sessionId, ct);
            return null;
        }

        var loginInfo = JsonConvert.DeserializeObject<LoginUserInfo>(session.LoginInfoJson);
        session.AccessTokenId = Guid.NewGuid().ToString("N");
        var pair = CreatePair(session, loginInfo!);
        if (!await sessions.RotateAsync(sessionId, previousHash, Hash(pair.RefreshToken), session.AccessTokenId, ct))
        {
            // 条件更新保证并发请求最多一个成功；重放会使先前签出的令牌同样失效。
            await sessions.RevokeAsync(sessionId, ct);
            return null;
        }

        return pair;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAccessAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        if (!TryGetIdentity(principal, JwtTokenValidation.AccessTokenUse, out var userId, out var sessionId,
                out var tokenId)) return false;
        var session = await sessions.FindActiveAsync(sessionId, ct);
        return session != null && session.UserId == userId && session.AccessTokenId == tokenId;
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        // 退出允许使用当前已过期的访问令牌；仍必须验签、校验用途及匹配服务端记录。
        var principal = ReadToken(token, JwtTokenValidation.RefreshTokenUse, false);
        var use = JwtTokenValidation.RefreshTokenUse;
        if (principal == null)
        {
            use = JwtTokenValidation.AccessTokenUse;
            principal = ReadToken(token, use, false);
        }

        if (!TryGetIdentity(principal, use, out var userId, out var sessionId, out var tokenId)) return;
        var session = await sessions.FindActiveAsync(sessionId, ct);
        if (session == null || session.UserId != userId) return;
        if ((use == JwtTokenValidation.AccessTokenUse && session.AccessTokenId == tokenId) ||
            use == JwtTokenValidation.RefreshTokenUse)
            await sessions.RevokeAsync(sessionId, ct);
    }

    private TokenVo CreatePair(AuthSession session, LoginUserInfo loginInfo)
    {
        var now = UtcNow;
        var accessExpires = new[] { now.AddMilliseconds(_options.AccessTokenLifetimeMs), session.ExpiresAtUtc }.Min();
        return new TokenVo
        {
            SessionId = session.Id,
            AccessToken = CreateToken(session, loginInfo, session.AccessTokenId,
                JwtTokenValidation.AccessTokenUse, now, accessExpires),
            RefreshToken = CreateToken(session, loginInfo, Guid.NewGuid().ToString("N"),
                JwtTokenValidation.RefreshTokenUse, now, session.ExpiresAtUtc),
            TokenType = AuthConstants.JwtTokenType,
            Expires = new DateTimeOffset(accessExpires, TimeSpan.Zero).ToUnixTimeMilliseconds(),
            RefreshTokenExpires = new DateTimeOffset(session.ExpiresAtUtc, TimeSpan.Zero).ToUnixTimeMilliseconds()
        };
    }

    private string CreateToken(AuthSession session, LoginUserInfo info, string id, string use,
        DateTime now, DateTime expires)
    {
        var claims = new List<Claim>
        {
            new("sub", session.UserId.ToString(CultureInfo.InvariantCulture)),
            new("sid", session.Id),
            new("jti", id),
            new("token_use", use),
            new("iat", new DateTimeOffset(now, TimeSpan.Zero).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),
            new("name", info.Account),
            new("dept_id", info.DeptId.ToString(CultureInfo.InvariantCulture))
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecurityKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(new JwtHeader(credentials)
            { ["typ"] = use == JwtTokenValidation.AccessTokenUse ? "at+jwt" : "rt+jwt" },
            new JwtPayload(_options.Issuer, _options.Audience, claims, now, expires));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ReadToken(string token, string use, bool validateLifetime)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 8192) return null;
        try
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var parameters = JwtTokenValidation.CreateParameters(_options, validateLifetime, use);
            if (validateLifetime)
                parameters.LifetimeValidator = (notBefore, expires, _, _) =>
                    expires.HasValue && expires.Value > UtcNow &&
                    (!notBefore.HasValue || notBefore <= UtcNow.AddSeconds(30));
            return handler.ValidateToken(token, parameters, out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }

    private static bool TryGetIdentity(ClaimsPrincipal? principal, string use, out long userId,
        out string sessionId, out string tokenId)
    {
        userId = 0;
        sessionId = principal?.FindFirst("sid")?.Value ?? string.Empty;
        tokenId = principal?.FindFirst("jti")?.Value ?? string.Empty;
        return principal?.Identity?.IsAuthenticated == true && principal.FindFirst("token_use")?.Value == use &&
               long.TryParse(principal.FindFirst("sub")?.Value, out userId) && userId > 0 &&
               Guid.TryParseExact(sessionId, "N", out _) && Guid.TryParseExact(tokenId, "N", out _);
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
