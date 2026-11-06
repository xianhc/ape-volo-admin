using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Ape.Volo.Core.Caches.Redis;

public class RedisCache : ICache, ICacheKeyCatalog
{


    /// <summary>
    /// 允许复用已有 Redis 连接；不接管连接生命周期。
    /// </summary>
    /// <param name="database">可复用的 Redis 数据库连接</param>
    public RedisCache(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async Task<bool> CompareExchangeAsync(string key, string? expected, string? value, TimeSpan timeout,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (value != null && timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        const string script = """
            local current = redis.call('GET', KEYS[1])
            if ARGV[1] == '0' then
                if current then return 0 end
            else
                if not current then return 0 end
                local entry = cjson.decode(current)
                if entry.typeName ~= ARGV[3] or entry.value ~= ARGV[2] then return 0 end
            end
            if ARGV[4] == '0' then
                redis.call('DEL', KEYS[1])
            else
                redis.call('SET', KEYS[1], ARGV[5], 'PX', ARGV[6])
            end
            return 1
            """;
        var entry = value == null ? null : new ValueInfoEntry
        {
            Value = value, TypeName = typeof(string).AssemblyQualifiedName!,
            ExpireTime = timeout, ExpireType = CacheExpireType.Absolute
        };
        var result = await _database.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[]
        {
            expected == null ? "0" : "1", expected ?? "", typeof(string).AssemblyQualifiedName,
            value == null ? "0" : "1", entry?.ToJson() ?? "",
            Math.Max(1L, (long)Math.Ceiling(timeout.TotalMilliseconds))
        }).WaitAsync(ct);
        return (long)result == 1;
    }

    private const int DefaultTimeout = 60 * 20;

    private IDatabase _database;

    private ConnectionMultiplexer? _connectionMultiplexer;

    /// <summary>
    /// 初始化 <see cref="RedisCache"/> 的新实例（按配置自建 Redis 连接）。
    /// </summary>
    /// <param name="systemOptions">系统配置</param>
    /// <param name="redisOptions">Redis 配置</param>
    public RedisCache(IOptions<SystemOptions> systemOptions, IOptions<RedisOptions> redisOptions)
    {
        if (!systemOptions.Value.UseRedisCache)
        {
            throw new Exception("UseRedisCache is not set to True, please check！");
        }

        var redis = redisOptions.Value;
        ThreadPool.SetMinThreads(200, 200);
        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = redis.AbortOnConnectFail,
            AllowAdmin = redis.AllowAdmin,
            ConnectRetry = redis.ConnectRetry,
            ConnectTimeout = redis.ConnectTimeout,
            KeepAlive = redis.KeepAlive,
            SyncTimeout = redis.SyncTimeout,
            EndPoints = { redis.Host + ":" + redis.Port },
            ServiceName = redis.Name.IsNullOrEmpty() ? null : redis.Name
        };
        if (!string.IsNullOrWhiteSpace(redis.Password))
        {
            options.Password = redis.Password;
        }

        _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        _database = _connectionMultiplexer.GetDatabase(redis.Index);
    }


    public IDatabase GetDatabase(int db = -1)
    {
        if (db == -1)
        {
            return _database;
        }

        return _database.Multiplexer.GetDatabase(db);
    }

    #region 获取缓存

    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Get<T>(string key)
    {
        var redisValue = _database.StringGet(key);
        if (!redisValue.HasValue)
            return default;
        var valueEntry = redisValue.ToString().ToObject<ValueInfoEntry>();
        if (valueEntry == null)
            return default;
        var value = valueEntry.TypeName == typeof(string).AssemblyQualifiedName
            ? valueEntry.Value
            : valueEntry.Value.ToObject(Type.GetType(valueEntry.TypeName) ??
                throw new InvalidOperationException("缓存条目中的类型无法解析。"));

        if (valueEntry.ExpireType == CacheExpireType.Relative)
            _database.KeyExpire(key, valueEntry.ExpireTime);
        return value == null ? default : (T)value;
    }


    /// <summary>
    /// 获取缓存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var redisValue = await _database.StringGetAsync(key).WaitAsync(ct);
        if (!redisValue.HasValue)
            return default;
        var valueEntry = redisValue.ToString().ToObject<ValueInfoEntry>();
        if (valueEntry == null)
            return default;
        var value = valueEntry.TypeName == typeof(string).AssemblyQualifiedName
            ? valueEntry.Value
            : valueEntry.Value.ToObject(Type.GetType(valueEntry.TypeName) ??
                throw new InvalidOperationException("缓存条目中的类型无法解析。"));

        if (valueEntry.ExpireType == CacheExpireType.Relative)
            await _database.KeyExpireAsync(key, valueEntry.ExpireTime).WaitAsync(ct);

        return value == null ? default : (T)value;
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
            TypeName = value.GetType().AssemblyQualifiedName!,
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
        CacheExpireType? cacheExpireType, CancellationToken ct = default)
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
        ct.ThrowIfCancellationRequested();
        return await _database.StringSetAsync(key, theValue, expireTime).WaitAsync(ct);
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
    public async Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await _database.KeyDeleteAsync(key).WaitAsync(ct);
    }

    #endregion

    /// <summary>
    /// 模糊查询key的集合
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<string[]> GetKeysAsync(string prefix, CancellationToken ct = default)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var connection = _database.Multiplexer;
        foreach (var endpoint in connection.GetEndPoints())
        {
            var server = connection.GetServer(endpoint);
            if (!server.IsConnected || server.IsReplica) continue;
            await foreach (var item in server.KeysAsync(_database.Database, pattern: prefix + "*", pageSize: 250).WithCancellation(ct))
                keys.Add(item.ToString());
        }
        return keys.ToArray();
    }
}
