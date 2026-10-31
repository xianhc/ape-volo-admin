using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.ConfigOptions;
using Newtonsoft.Json;
using SqlSugar;
using StackExchange.Redis;
using Ape.Volo.Core.Caches.Redis;

namespace Ape.Volo.Core.Caches.SqlSugar;

/// <summary>
/// redis缓存
/// 实现SqlSugar.ICacheService
/// </summary>
public class SqlSugarRedisCache : ICacheService
{
    private readonly RedisCache _cache;
    private readonly int _databaseIndex;

    /// <summary>
    /// 初始化 <see cref="SqlSugarRedisCache"/> 的新实例。
    /// </summary>
    /// <param name="cache">缓存实现</param>
    /// <param name="redisOptions">Redis 配置</param>
    public SqlSugarRedisCache(RedisCache cache, RedisOptions redisOptions)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _databaseIndex = redisOptions.Index + 1;
    }

    private IDatabase Database => _cache.GetDatabase(_databaseIndex);


    public void Add<TV>(string key, TV value)
    {
        var valStr = value.ToJson();
        Database.StringSet(key, valStr);
    }

    public void Add<TV>(string key, TV value, int cacheDurationInSeconds)
    {
        var valStr = value.ToJson();
        var expireTime = new TimeSpan(0, 0, 0, cacheDurationInSeconds);
        Database.StringSet(key, valStr, expireTime);
    }

    public bool ContainsKey<TV>(string key)
    {
        return Database.KeyExists(key);
    }

    public TV Get<TV>(string key)
    {
        var val = Database.StringGet(key);
        var redisValue = Database.StringGet(key);
        if (!redisValue.HasValue)
            return default!;
        return JsonConvert.DeserializeObject<TV>(val!)!;
    }

    public IEnumerable<string> GetAllKey<TV>()
    {
        var pattern = "SqlSugarDataCache.*";
        var redisResult = Database.ScriptEvaluate(LuaScript.Prepare(
            " local res = redis.call('KEYS', @keypattern) " +
            " return res "), new { keypattern = pattern });
        string[] preSult = (string[])redisResult!;
        return preSult;
    }

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
        Database.KeyDelete(key);
    }
}
