using System.Text;
using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;


namespace Ape.Volo.Core.Caches.Distributed;

public class DistributedCache : ICache
{
    // 当前实现包装内置 MemoryDistributedCache；固定条带锁在多个包装实例之间共享，且不随键数增长。
    private static readonly SemaphoreSlim[] MutationLocks = Enumerable.Range(0, 128)
        .Select(_ => new SemaphoreSlim(1, 1)).ToArray();


    public async Task<bool> CompareExchangeAsync(string key, string? expected, string? value, TimeSpan timeout,
        CancellationToken ct = default)
    {
        if (value != null && timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        var gate = MutationLocks[(StringComparer.Ordinal.GetHashCode(key) & int.MaxValue) % MutationLocks.Length];
        await gate.WaitAsync(ct);
        try
        {
            var bytes = await _cache.GetAsync(key, ct);
            var current = bytes == null ? null : Encoding.UTF8.GetString(bytes).ToObject<ValueInfoEntry>();
            if (expected == null ? current != null :
                current == null || current.TypeName != typeof(string).AssemblyQualifiedName || current.Value != expected)
                return false;

            if (value == null) await _cache.RemoveAsync(key, ct);
            else
            {
                var entry = new ValueInfoEntry
                {
                    Value = value, TypeName = typeof(string).AssemblyQualifiedName!,
                    ExpireTime = timeout, ExpireType = CacheExpireType.Absolute
                };
                await _cache.SetAsync(key, Encoding.UTF8.GetBytes(entry.ToJson()),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = timeout }, ct);
            }
            return true;
        }
        finally { gate.Release(); }
    }

    private readonly IDistributedCache _cache;
    private const int DefaultTimeout = 60 * 20;

    /// <summary>
    /// 初始化 <see cref="DistributedCache"/> 的新实例。
    /// </summary>
    /// <param name="cache">cache</param>

    public DistributedCache(IDistributedCache cache)
    {
        _cache = cache;
    }


    public T? Get<T>(string key)
    {
        var cacheValue = _cache.Get(key);
        if (cacheValue.IsNullOrEmpty())
            return default;
        var valueEntry = Encoding.UTF8.GetString(cacheValue, 0, cacheValue.Length).ToObject<ValueInfoEntry>();
        if (valueEntry == null)
            return default;
        var result = valueEntry.TypeName == typeof(string).AssemblyQualifiedName
            ? valueEntry.Value
            : valueEntry.Value.ToObject(Type.GetType(valueEntry.TypeName) ??
                throw new InvalidOperationException("缓存条目中的类型无法解析。"));

        if (valueEntry.ExpireType == CacheExpireType.Relative)
        {
            _cache.Remove(key);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = valueEntry.ExpireTime };
            _cache.Set(key, cacheValue, options);
            //_cache.Refresh(key);
        }

        return result == null ? default : (T)result;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var cacheValue = await _cache.GetAsync(key, ct);
        if (cacheValue.IsNullOrEmpty())
            return default;
        var valueEntry = Encoding.UTF8.GetString(cacheValue, 0, cacheValue.Length).ToObject<ValueInfoEntry>();
        if (valueEntry == null)
            return default;
        var result = valueEntry.TypeName == typeof(string).AssemblyQualifiedName
            ? valueEntry.Value
            : valueEntry.Value.ToObject(Type.GetType(valueEntry.TypeName) ??
                throw new InvalidOperationException("缓存条目中的类型无法解析。"));

        if (valueEntry.ExpireType == CacheExpireType.Relative)
        {
            await _cache.RemoveAsync(key, ct);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = valueEntry.ExpireTime };
            await _cache.SetAsync(key, cacheValue, options, ct);
            // await _cache.RefreshAsync(key);
        }

        return result == null ? default : (T)result;
    }

    public bool Set(string key, object value, TimeSpan? timeSpan, CacheExpireType? cacheExpireType)
    {
        string jsonStr;
        if (value is string s)
            jsonStr = s;
        else
            jsonStr = value.ToJson();
        var expireTime = timeSpan ?? new TimeSpan(0, 0, 0, DefaultTimeout);
        var entry = new ValueInfoEntry
        {
            Value = jsonStr,
            TypeName = value.GetType().AssemblyQualifiedName!,
            ExpireTime = expireTime,
            ExpireType = cacheExpireType ?? CacheExpireType.Absolute
        };
        var theValue = entry.ToJson();
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expireTime };
        _cache.Set(key, Encoding.UTF8.GetBytes(theValue), options);
        return true;
    }

    public async Task<bool> SetAsync(string key, object value, TimeSpan? timeSpan, CacheExpireType? cacheExpireType, CancellationToken ct = default)
    {
        string jsonStr;
        if (value is string s)
            jsonStr = s;
        else
            jsonStr = value.ToJson();
        var expireTime = timeSpan ?? new TimeSpan(0, 0, 0, DefaultTimeout);
        var entry = new ValueInfoEntry
        {
            Value = jsonStr,
            TypeName = value.GetType().AssemblyQualifiedName!,
            ExpireTime = expireTime,
            ExpireType = cacheExpireType ?? CacheExpireType.Absolute
        };
        var theValue = entry.ToJson();
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expireTime };
        await _cache.SetAsync(key, Encoding.UTF8.GetBytes(theValue), options, ct);
        return true;
    }

    public bool Remove(string key)
    {
        _cache.Remove(key);
        return true;
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(key, ct);
        return true;
    }

}
