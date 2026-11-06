using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Core.ConfigOptions;
using Newtonsoft.Json;

namespace Ape.Volo.Application.Identity.Auth;

/// <summary>
/// 会话与用户撤销版本都使用系统缓存；正常鉴权不访问数据库。
/// </summary>
public class AuthSessionService(ICache cache, IOptions<JwtAuthOptions> options, TimeProvider clock, IEnumerable<ICacheKeyCatalog> keyCatalogs) : IAuthSessionService
{
    /// <summary>
    /// 会话缓存的键前缀
    /// </summary>
    public const string SessionKeyPrefix = "auth:session:";
    private const string UserVersionPrefix = "auth:user-version:";
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        DateFormatString = "O", DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        TypeNameHandling = TypeNameHandling.None, NullValueHandling = NullValueHandling.Include
    };
    private DateTime UtcNow => clock.GetUtcNow().UtcDateTime;

    /// <inheritdoc />
    public async Task<bool> CreateAsync(AuthSession session, CancellationToken ct = default)
    {
        var remaining = session.ExpiresAtUtc - UtcNow;
        if (session.UserId <= 0 || remaining <= TimeSpan.Zero) return false;
        var versionKey = UserVersionPrefix + session.UserId;
        // 登录可延长用户版本键的 TTL，但不能覆盖并发的用户撤销操作。
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var previous = await cache.GetAsync<string>(versionKey);
            var version = previous ?? Guid.NewGuid().ToString("N");
            if (!await cache.CompareExchangeAsync(versionKey, previous!, version, VersionLifetime(remaining), ct))
                continue;
            session.UserVersion = version;
            break;
        }
        var created = await cache.CompareExchangeAsync(SessionKeyPrefix + session.Id, null,
            Serialize(session), remaining, ct);
        return created && await cache.GetAsync<string>(versionKey) == session.UserVersion;
    }

    /// <inheritdoc />
    public async Task<AuthSession?> GetActiveAsync(string sessionId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(sessionId)) return null;
        var raw = await cache.GetAsync<string>(SessionKeyPrefix + sessionId);
        if (raw == null) return null; // 缓存丢失时必须重新登录，绝不重建会话。
        var session = Deserialize(raw);
        if (session == null || session.ExpiresAtUtc <= UtcNow) return null;
        var version = await cache.GetAsync<string>(UserVersionPrefix + session.UserId);
        return version != null && version == session.UserVersion ? session : null;
    }

    /// <inheritdoc />
    public async Task<bool> RotateAsync(string sessionId, string previousHash, string nextHash,
        string accessTokenId, CancellationToken ct = default)
    {
        var key = SessionKeyPrefix + sessionId;
        var previous = await cache.GetAsync<string>(key);
        if (previous == null) return false;
        var session = Deserialize(previous);
        if (session == null) return false;
        var remaining = session.ExpiresAtUtc - UtcNow;
        if (remaining <= TimeSpan.Zero || session.RefreshTokenHash != previousHash) return false;
        var version = await cache.GetAsync<string>(UserVersionPrefix + session.UserId);
        if (version == null || version != session.UserVersion) return false;
        session.RefreshTokenHash = nextHash;
        session.AccessTokenId = accessTokenId;
        return await cache.CompareExchangeAsync(key, previous, Serialize(session), remaining, ct);
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string sessionId, CancellationToken ct = default)
    {
        var key = SessionKeyPrefix + sessionId;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var previous = await cache.GetAsync<string>(key);
            if (previous == null) return;
            // 与刷新共用原子操作，避免先删除再被并发刷新写回。
            if (await cache.CompareExchangeAsync(key, previous, null, TimeSpan.Zero, ct)) return;
        }
    }

    /// <inheritdoc />
    public async Task RevokeUserAsync(long userId, CancellationToken ct = default)
    {
        var key = UserVersionPrefix + userId;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var previous = await cache.GetAsync<string>(key);
            if (await cache.CompareExchangeAsync(key, previous, Guid.NewGuid().ToString("N"),
                    VersionLifetime(TimeSpan.Zero), ct)) return;
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

    private TimeSpan VersionLifetime(TimeSpan minimum) =>
        TimeSpan.FromTicks(Math.Max(minimum.Ticks, TimeSpan.FromMilliseconds(options.Value.RefreshTokenLifetimeMs).Ticks));
    private static string Serialize(AuthSession session) => JsonConvert.SerializeObject(session, JsonSettings);
    private static AuthSession? Deserialize(string value) => JsonConvert.DeserializeObject<AuthSession>(value, JsonSettings);
}
