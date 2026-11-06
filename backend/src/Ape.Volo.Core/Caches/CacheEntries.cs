using Newtonsoft.Json;
using Ape.Volo.Common.Extensions;

namespace Ape.Volo.Core.Caches;

/// <summary>
/// 通过原子比较交换协调缓存加载与失效，内存和 Redis 使用相同协议。
/// </summary>
public static class CacheEntries
{
    private sealed record Entry(string Revision, string? Type = null, string? Json = null);
    private static string Empty() => JsonConvert.SerializeObject(new Entry(Guid.NewGuid().ToString("N")));

    /// <summary>读取结果；加载期间发生失效或过期时重新读取，不发布旧结果。</summary>
    /// <typeparam name="T">结果类型。</typeparam>
    /// <param name="cache">缓存存储。</param>
    /// <param name="key">逻辑缓存键。</param>
    /// <param name="load">未命中时的数据读取。</param>
    /// <param name="lifetime">绝对有效期。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>与当前缓存修订一致的结果。</returns>
    public static async Task<T> GetOrCreateAsync<T>(this ICache cache, string key, Func<Task<T>> load,
        TimeSpan lifetime, CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var observed = await cache.GetAsync<string>(key, ct);
            if (observed == null)
            {
                observed = Empty();
                if (!await cache.CompareExchangeAsync(key, null, observed, lifetime, ct)) continue;
            }
            var entry = JsonConvert.DeserializeObject<Entry>(observed)!;
            if (entry.Type == typeof(T).AssemblyQualifiedName && entry.Json != null)
                return JsonConvert.DeserializeObject<T>(entry.Json)!;

            var result = await load();
            ct.ThrowIfCancellationRequested();
            var next = JsonConvert.SerializeObject(entry with
            {
                Type = typeof(T).AssemblyQualifiedName, Json = result.ToJson()
            });
            if (await cache.CompareExchangeAsync(key, observed, next, lifetime, ct)) return result;
        }
    }

    /// <summary>原子推进缓存修订；已在加载的旧结果无法覆盖此修订。</summary>
    /// <param name="cache">缓存存储。</param>
    /// <param name="key">逻辑缓存键。</param>
    /// <param name="ct">取消令牌；提交后的必要清理使用默认值。</param>
    public static async Task InvalidateAsync(this ICache cache, string key, CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var previous = await cache.GetAsync<string>(key, ct);
            if (await cache.CompareExchangeAsync(key, previous, Empty(), TimeSpan.FromHours(2), ct)) break;
        }
    }
}
