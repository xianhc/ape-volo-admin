using Microsoft.Extensions.Caching.Memory;

namespace Ape.Volo.Core.Caches.Distributed;

/// <summary>进程内缓存：业务值直接保存为字符串或 JSON，过期由后端管理。</summary>
public class DistributedCache(MemoryCache cache) : ICache, ICacheKeyCatalog
{
    // 多个包装实例共用同一组固定条带锁，协调原子比较与写入。
    private static readonly SemaphoreSlim[] MutationLocks = Enumerable.Range(0, 128)
        .Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    private static SemaphoreSlim Gate(string key) =>
        MutationLocks[(StringComparer.Ordinal.GetHashCode(key) & int.MaxValue) % MutationLocks.Length];

    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        var gate = Gate(key);
        gate.Wait();
        try { return CacheValues.Deserialize<T>(cache.Get<string>(key)); }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var gate = Gate(key);
        await gate.WaitAsync(ct);
        try { return CacheValues.Deserialize<T>(cache.Get<string>(key)); }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public bool Set(string key, object value, TimeSpan? timeout, CacheExpireType? cacheExpireType)
    {
        var gate = Gate(key);
        gate.Wait();
        try { cache.Set(key, CacheValues.Serialize(value), Options(timeout, cacheExpireType)); }
        finally { gate.Release(); }
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> SetAsync(string key, object value, TimeSpan? timeout,
        CacheExpireType? cacheExpireType, CancellationToken ct = default)
    {
        var gate = Gate(key);
        await gate.WaitAsync(ct);
        try { cache.Set(key, CacheValues.Serialize(value), Options(timeout, cacheExpireType)); }
        finally { gate.Release(); }
        return true;
    }

    /// <inheritdoc />
    public bool Remove(string key)
    {
        var gate = Gate(key);
        gate.Wait();
        try { cache.Remove(key); }
        finally { gate.Release(); }
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        var gate = Gate(key);
        await gate.WaitAsync(ct);
        try { cache.Remove(key); }
        finally { gate.Release(); }
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> CompareExchangeAsync(string key, string? expected, string? value, TimeSpan timeout,
        CancellationToken ct = default)
    {
        if (value != null && timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        var gate = Gate(key);
        await gate.WaitAsync(ct);
        try
        {
            if (cache.Get<string>(key) != expected) return false;
            if (value == null) cache.Remove(key);
            else cache.Set(key, value, Options(timeout, null));
            return true;
        }
        finally { gate.Release(); }
    }

    /// <inheritdoc />
    public Task<string[]> GetKeysAsync(string prefix, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(cache.Keys.OfType<string>()
            .Where(key => key.StartsWith(prefix, StringComparison.Ordinal) && cache.TryGetValue(key, out _))
            .ToArray());
    }

    private static MemoryCacheEntryOptions Options(TimeSpan? timeout, CacheExpireType? expireType)
    {
        var lifetime = timeout ?? TimeSpan.FromMinutes(20);
        if (lifetime <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        return expireType == CacheExpireType.Relative
            ? new MemoryCacheEntryOptions { SlidingExpiration = lifetime }
            : new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = lifetime };
    }
}
