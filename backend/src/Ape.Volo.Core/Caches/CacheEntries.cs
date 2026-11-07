namespace Ape.Volo.Core.Caches;

/// <summary>查询缓存未命中时加载并保存业务值。</summary>
public static class CacheEntries
{
    /// <summary>读取缓存结果，未命中时加载并写入缓存。</summary>
    /// <typeparam name="T">结果类型。</typeparam>
    /// <param name="cache">缓存存储。</param>
    /// <param name="key">逻辑缓存键。</param>
    /// <param name="load">未命中时的数据读取。</param>
    /// <param name="lifetime">绝对有效期。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>缓存或加载的结果。</returns>
    public static async Task<T> GetOrCreateAsync<T>(this ICache cache, string key, Func<Task<T>> load,
        TimeSpan lifetime, CancellationToken ct = default)
    {
        var cached = await cache.GetAsync<string>(key, ct);
        if (cached != null)
            return CacheValues.DeserializeJson<T>(cached)!;

        var result = await load();
        ct.ThrowIfCancellationRequested();
        await cache.SetAsync(key, CacheValues.SerializeJson(result), lifetime, CacheExpireType.Absolute, ct);
        return result;
    }
}
