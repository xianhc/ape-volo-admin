using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.ConfigOptions;
using StackExchange.Redis;

namespace Ape.Volo.Core.Caches.Redis;

public class RedisCache : ICache
{
    private const int DefaultTimeout = 60 * 20;

    private IDatabase _database;

    private ConnectionMultiplexer _connectionMultiplexer;
    //private static ISubscriber _sub;

    public RedisCache()
    {
        if (!App.GetOptions<SystemOptions>().UseRedisCache)
        {
            throw new System.Exception("UseRedisCache is not set to True, please check！");
        }

        var redisOptions = App.GetOptions<RedisOptions>();
        ThreadPool.SetMinThreads(200, 200);
        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = redisOptions.AbortOnConnectFail,
            AllowAdmin = redisOptions.AllowAdmin,
            ConnectRetry = redisOptions.ConnectRetry,
            ConnectTimeout = redisOptions.ConnectTimeout,
            KeepAlive = redisOptions.KeepAlive,
            SyncTimeout = redisOptions.SyncTimeout,
            EndPoints = { redisOptions.Host + ":" + redisOptions.Port },
            ServiceName = redisOptions.Name.IsNullOrEmpty() ? null : redisOptions.Name
        };
        if (!string.IsNullOrWhiteSpace(redisOptions.Password))
        {
            options.Password = redisOptions.Password;
        }

        _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        _database = _connectionMultiplexer.GetDatabase(redisOptions.Index);
    }


    public IDatabase GetDatabase(int db = -1)
    {
        if (db == -1)
        {
            return _database;
        }

        return _connectionMultiplexer.GetDatabase(db);
    }

    #region 获取缓存

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(string key)
    {
        object value = null;
        var redisValue = _database.StringGet(key);
        if (!redisValue.HasValue)
            return default;
        var valueEntry = redisValue.ToString().ToObject<ValueInfoEntry>();
        value = valueEntry.TypeName == typeof(string).AssemblyQualifiedName
            ? valueEntry.Value
            : valueEntry.Value.ToObject(Type.GetType(valueEntry.TypeName));

        if (valueEntry.ExpireType == CacheExpireType.Relative)
            _database.KeyExpire(key, valueEntry.ExpireTime);
        return (T)value;
    }


    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(string key)
    {
        object value = null;
        var redisValue = await _database.StringGetAsync(key);
        if (!redisValue.HasValue)
            return default;
        var valueEntry = redisValue.ToString().ToObject<ValueInfoEntry>();
        value = valueEntry.TypeName == typeof(string).AssemblyQualifiedName
            ? valueEntry.Value
            : valueEntry.Value.ToObject(Type.GetType(valueEntry.TypeName));

        if (valueEntry.ExpireType == CacheExpireType.Relative)
            await _database.KeyExpireAsync(key, valueEntry.ExpireTime);

        return (T)value;
    }

    #endregion

    #region 添加缓存

    /// <summary>
    /// 添加缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="timeSpan">过期时间</param>
    /// <param name="cacheExpireType">过期类型</param>
    /// <returns>添加结果</returns>
    public bool Set(string key, object value, TimeSpan? timeSpan,
        CacheExpireType? cacheExpireType)
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
            TypeName = value.GetType().AssemblyQualifiedName,
            ExpireTime = expireTime,
            ExpireType = cacheExpireType ?? CacheExpireType.Absolute
        };
        var theValue = entry.ToJson();
        return _database.StringSet(key, theValue, expireTime);
    }


    /// <summary>
    /// 添加缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="timeSpan">过期时间</param>
    /// <param name="cacheExpireType">过期类型</param>
    /// <returns>添加结果</returns>
    public async Task<bool> SetAsync(string key, object value, TimeSpan? timeSpan,
        CacheExpireType? cacheExpireType)
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
            TypeName = value.GetType().AssemblyQualifiedName,
            ExpireTime = expireTime,
            ExpireType = cacheExpireType ?? CacheExpireType.Absolute
        };
        var theValue = entry.ToJson();
        return await _database.StringSetAsync(key, theValue, expireTime);
    }

    #endregion

    #region 移除缓存

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>移除结果</returns>
    public bool Remove(string key)
    {
        return _database.KeyDelete(key);
    }

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <returns>移除结果</returns>
    public async Task<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    #endregion

    /// <summary>
    /// 模糊查询key的集合
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<string[]> ScriptEvaluateKeys(string key)
    {
        var pattern = $"{key}*"; //匹配符
        var redisResult = await _database.ScriptEvaluateAsync(LuaScript.Prepare(
            //Redis的keys模糊查询：
            " local res = redis.call('KEYS', @keypattern) " +
            " return res "), new { keypattern = pattern });
        string[] preSult = (string[])redisResult; //将返回的结果集转为数组
        return preSult;
    }
}