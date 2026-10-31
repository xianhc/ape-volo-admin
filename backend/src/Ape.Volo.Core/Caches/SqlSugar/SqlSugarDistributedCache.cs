using Ape.Volo.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SqlSugar;

namespace Ape.Volo.Core.Caches.SqlSugar;

/// <summary>
/// Distributed缓存
/// 实现SqlSugar.ICacheService
/// </summary>
public class SqlSugarDistributedCache : ICacheService
{
    private readonly IDistributedCache _caching;

    /// <summary>
    /// 初始化 <see cref="SqlSugarDistributedCache"/> 的新实例。
    /// </summary>
    /// <param name="caching">分布式缓存</param>
    public SqlSugarDistributedCache(IDistributedCache caching)
    {
        _caching = caching ?? throw new ArgumentNullException(nameof(caching));
    }


    public void Add<TV>(string key, TV value)
    {
        var valStr = value?.ToJson() ?? string.Empty;
        _caching.SetString(key, valStr);
    }

    public void Add<TV>(string key, TV value, int cacheDurationInSeconds)
    {
        var valStr = value?.ToJson() ?? string.Empty;
        DistributedCacheEntryOptions op = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheDurationInSeconds),
        };
        _caching.SetString(key, valStr, op);
    }

    public bool ContainsKey<TV>(string key)
    {
        return _caching.Get(key) != null;
    }

    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public TV Get<TV>(string key)
    {
        var val = _caching.GetString(key);
        if (val == null) return default;
        return JsonConvert.DeserializeObject<TV>(val);
    }

    public IEnumerable<string> GetAllKey<TV>()
    {
        //throw new NotImplementedException("IDistributedCache不支持模糊查询KEY");
        //实现思路 创建一个缓存区。然后添加缓存时，把key增加到这个缓存区
        //每次获取时都需要检查一下缓存是否存在 不存在则删除
        //这个获取所有key 获取这个定义得缓存区就行了 
        return new[] { "" };
    }

    [return: System.Diagnostics.CodeAnalysis.MaybeNull]
    public TV GetOrCreate<TV>(string cacheKey, Func<TV> create, int cacheDurationInSeconds = int.MaxValue)
    {
        if (ContainsKey<TV>(cacheKey))
        {
            return Get<TV>(cacheKey);
        }

        TV val = create();
        Add(cacheKey, val, cacheDurationInSeconds);
        return val;
    }

    public void Remove<TV>(string key)
    {
        _caching.Remove(key);
    }
}
