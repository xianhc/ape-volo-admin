using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Ape.Volo.Core.Caches.Redis;

/// <summary>Redis 缓存：字符串保存原文，对象保存 JSON，不写入 CLR 类型信息。</summary>
public class RedisCache : ICache, ICacheKeyCatalog
{
    private readonly IDatabase _database;
    private readonly ConnectionMultiplexer? _connectionMultiplexer;

    // 滑动过期配置单独保存；读取与续期在同一脚本内完成。
    private const string ReadScript = """
        local value = redis.call('GET', KEYS[1])
        if value then
            local sliding = redis.call('GET', KEYS[2])
            if sliding then
                redis.call('PEXPIRE', KEYS[1], sliding)
                redis.call('PEXPIRE', KEYS[2], sliding)
            end
        end
        return value
        """;

    private const string SetScript = """
        redis.call('SET', KEYS[1], ARGV[1], 'PX', ARGV[2])
        if ARGV[3] == '1' then
            redis.call('SET', KEYS[2], ARGV[2], 'PX', ARGV[2])
        else
            redis.call('DEL', KEYS[2])
        end
        return 1
        """;

    /// <summary>复用已有 Redis 连接，不接管连接生命周期。</summary>
    /// <param name="database">Redis 数据库连接。</param>
    public RedisCache(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>按配置创建 Redis 连接。</summary>
    /// <param name="systemOptions">系统配置。</param>
    /// <param name="redisOptions">Redis 配置。</param>
    public RedisCache(IOptions<SystemOptions> systemOptions, IOptions<RedisOptions> redisOptions)
    {
        if (!systemOptions.Value.UseRedisCache)
            throw new Exception("UseRedisCache is not set to True, please check！");

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
        if (!string.IsNullOrWhiteSpace(redis.Password)) options.Password = redis.Password;
        _connectionMultiplexer = ConnectionMultiplexer.Connect(options);
        _database = _connectionMultiplexer.GetDatabase(redis.Index);
    }

    /// <summary>获取当前或指定编号的 Redis 数据库。</summary>
    /// <param name="db">数据库编号，-1 表示当前数据库。</param>
    /// <returns>数据库连接。</returns>
    public IDatabase GetDatabase(int db = -1) => db == -1 ? _database : _database.Multiplexer.GetDatabase(db);

    /// <inheritdoc />
    public T? Get<T>(string key) => CacheValues.Deserialize<T>((string?)_database.ScriptEvaluate(ReadScript, Keys(key)));

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var value = await _database.ScriptEvaluateAsync(ReadScript, Keys(key)).WaitAsync(ct);
        return CacheValues.Deserialize<T>((string?)value);
    }

    /// <inheritdoc />
    public bool Set(string key, object value, TimeSpan? timeout, CacheExpireType? cacheExpireType) =>
        (long)_database.ScriptEvaluate(SetScript, Keys(key), SetArguments(value, timeout, cacheExpireType)) == 1;

    /// <inheritdoc />
    public async Task<bool> SetAsync(string key, object value, TimeSpan? timeout,
        CacheExpireType? cacheExpireType, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var result = await _database.ScriptEvaluateAsync(SetScript, Keys(key),
            SetArguments(value, timeout, cacheExpireType)).WaitAsync(ct);
        return (long)result == 1;
    }

    /// <inheritdoc />
    public bool Remove(string key) => _database.KeyDelete(Keys(key)) > 0;

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string key, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return await _database.KeyDeleteAsync(Keys(key)).WaitAsync(ct) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> CompareExchangeAsync(string key, string? expected, string? value, TimeSpan timeout,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (value != null && timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        const string script = """
            local current = redis.call('GET', KEYS[1])
            if ARGV[1] == '0' then
                if current then return 0 end
            elseif not current or current ~= ARGV[2] then
                return 0
            end
            if ARGV[3] == '0' then
                redis.call('DEL', KEYS[1])
            else
                redis.call('SET', KEYS[1], ARGV[4], 'PX', ARGV[5])
            end
            redis.call('DEL', KEYS[2])
            return 1
            """;
        var result = await _database.ScriptEvaluateAsync(script, Keys(key), new RedisValue[]
        {
            expected == null ? "0" : "1", expected ?? "", value == null ? "0" : "1", value ?? "",
            value == null ? 1 : Milliseconds(timeout)
        }).WaitAsync(ct);
        return (long)result == 1;
    }

    /// <inheritdoc />
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

    private static RedisKey[] Keys(string key) => [key, key + ":sliding"];
    private static RedisValue[] SetArguments(object value, TimeSpan? timeout, CacheExpireType? expireType) =>
        [CacheValues.Serialize(value), Milliseconds(timeout ?? TimeSpan.FromMinutes(20)),
            expireType == CacheExpireType.Relative ? "1" : "0"];

    private static long Milliseconds(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        return Math.Max(1L, (long)Math.Ceiling(timeout.TotalMilliseconds));
    }
}
