using Ape.Volo.Shared.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ape.Volo.Application.Identity.Auth;

/// <summary>
/// 登录会话使用系统缓存；正常鉴权不访问数据库。
/// </summary>
public class AuthSessionService(ICache cache, TimeProvider clock, IEnumerable<ICacheKeyCatalog> keyCatalogs) : IAuthSessionService
{
    /// <summary>
    /// 会话缓存的键前缀
    /// </summary>
    public const string SessionKeyPrefix = "auth:session:";
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        DateFormatString = "O", DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        TypeNameHandling = TypeNameHandling.None, NullValueHandling = NullValueHandling.Include
    };
    private DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    /// <inheritdoc />
    public async Task<bool> CreateAsync(AuthSession session, CancellationToken ct = default)
    {
        var remaining = session.ExpiresAtUtc - UtcNow;
        if (session.UserId <= 0 || remaining <= TimeSpan.Zero) return false;
        return await cache.CompareExchangeAsync(SessionKeyPrefix + session.Id, null,
            Serialize(session), remaining, ct);
    }

    /// <inheritdoc />
    public async Task<AuthSession?> GetActiveAsync(string sessionId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(sessionId)) return null;
        var raw = await cache.GetAsync<string>(SessionKeyPrefix + sessionId, ct);
        if (raw == null) return null; // 缓存丢失时必须重新登录，绝不重建会话。
        var session = Deserialize(raw);
        return session != null && session.ExpiresAtUtc > UtcNow ? session : null;
    }

    /// <inheritdoc />
    public async Task<bool> RotateAsync(string sessionId, string previousHash, string nextHash,
        string accessTokenId, CancellationToken ct = default)
    {
        var key = SessionKeyPrefix + sessionId;
        var previous = await cache.GetAsync<string>(key, ct);
        if (previous == null) return false;
        var session = Deserialize(previous);
        if (session == null) return false;
        var remaining = session.ExpiresAtUtc - UtcNow;
        if (remaining <= TimeSpan.Zero || session.RefreshTokenHash != previousHash) return false;
        session.RefreshTokenHash = nextHash;
        session.AccessTokenId = accessTokenId;
        return await cache.CompareExchangeAsync(key, previous, Serialize(session), remaining, ct);
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string sessionId, CancellationToken ct = default)
    {
        await cache.RemoveAsync(SessionKeyPrefix + sessionId, ct);
    }

    /// <inheritdoc />
    public async Task RevokeUserAsync(long userId, CancellationToken ct = default)
    {
        var catalog = keyCatalogs.SingleOrDefault() ?? (ICacheKeyCatalog)cache;
        var keys = await catalog.GetKeysAsync(SessionKeyPrefix, ct);
        foreach (var key in keys)
        {
            var session = await GetActiveAsync(key[SessionKeyPrefix.Length..], ct);
            if (session?.UserId == userId) await cache.RemoveAsync(key, ct);
        }
    }

    /// <inheritdoc />
    public async Task<List<AuthSession>> ListActiveAsync(CancellationToken ct = default)
    {
        var catalog = keyCatalogs.SingleOrDefault();
        if (catalog == null)
            throw new BadRequestException("在线用户功能仅支持 Redis，请启用 UseRedisCache。");
        var keys = await catalog.GetKeysAsync(SessionKeyPrefix, ct);
        var sessions = new List<AuthSession>();
        foreach (var key in keys)
        {
            ct.ThrowIfCancellationRequested();
            var session = await GetActiveAsync(key[SessionKeyPrefix.Length..], ct);
            if (session != null) sessions.Add(session);
        }
        return sessions.OrderByDescending(x => x.CreatedAtUtc).ToList();
    }

    private static string Serialize(AuthSession session) => JsonConvert.SerializeObject(session, JsonSettings);
    private static AuthSession? Deserialize(string value) => JsonConvert.DeserializeObject<AuthSession>(value, JsonSettings);
}
