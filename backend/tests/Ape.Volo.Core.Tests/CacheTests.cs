using Ape.Volo.Application.Contracts.Abstractions.Caching;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Core.Aop;
using Ape.Volo.Core.Caches;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Shared.Enums;
using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Xunit;
using MemoryCache = Ape.Volo.Core.Caches.Distributed.DistributedCache;
using RedisCache = Ape.Volo.Core.Caches.Redis.RedisCache;

namespace Ape.Volo.Core.Tests;

public class CacheTests
{
    public static IEnumerable<object?[]> Backends()
    {
        yield return [null];
        if (Environment.GetEnvironmentVariable("APEVOLO_TEST_REDIS") is { Length: > 0 } connection)
            yield return [connection];
    }

    [Theory]
    [MemberData(nameof(Backends))]
    public async Task StoresBusinessJsonAndReadsUsingRequestedType(string? connection)
    {
        using var fixture = new CacheFixture(connection);
        var key = fixture.Key("payload");
        await fixture.Cache.SetAsync(key, new Payload(42, [7, 8]), TimeSpan.FromMinutes(1), null);

        Assert.True(JToken.DeepEquals(JToken.Parse("{\"id\":42,\"userIds\":[7,8]}"),
            JToken.Parse(fixture.Raw(key)!)));
        var result = await fixture.Cache.GetAsync<Payload>(key);
        Assert.Equal(42, result!.Id);
        Assert.Equal(new long[] { 7, 8 }, result.UserIds);

        fixture.Cache.Set(key, new Payload(43, []), TimeSpan.FromMinutes(1), null);
        Assert.Equal(43, fixture.Cache.Get<Payload>(key)!.Id);
    }

    [Theory]
    [MemberData(nameof(Backends))]
    public async Task CompareExchangeUsesRawValuesAndHasOneWinner(string? connection)
    {
        using var fixture = new CacheFixture(connection);
        var key = fixture.Key("session");
        const string initial = "{\"userId\":42,\"refreshTokenHash\":\"old\"}";
        Assert.True(await fixture.Cache.CompareExchangeAsync(key, null, initial, TimeSpan.FromMinutes(1)));
        Assert.Equal(initial, fixture.Raw(key));
        var results = await Task.WhenAll(
            fixture.Cache.CompareExchangeAsync(key, initial, "next-a", TimeSpan.FromMinutes(1)),
            fixture.Cache.CompareExchangeAsync(key, initial, "next-b", TimeSpan.FromMinutes(1)));
        Assert.Single(results.Where(success => success));
        Assert.False(await fixture.Cache.CompareExchangeAsync(key, initial, null, TimeSpan.Zero));
        var current = await fixture.Cache.GetAsync<string>(key);
        Assert.True(await fixture.Cache.CompareExchangeAsync(key, current, null, TimeSpan.Zero));
        Assert.Null(fixture.Raw(key));
    }

    [Theory]
    [MemberData(nameof(Backends))]
    public async Task GetOrCreateStoresOnlyBusinessValueAndCachesNull(string? connection)
    {
        using var fixture = new CacheFixture(connection);
        var key = fixture.Key("query");
        var loads = 0;
        Task<Payload> Load() { loads++; return Task.FromResult(new Payload(42, [7])); }
        await fixture.Cache.GetOrCreateAsync(key, Load, TimeSpan.FromMinutes(1));
        var hit = await fixture.Cache.GetOrCreateAsync(key, Load, TimeSpan.FromMinutes(1));
        Assert.Equal(1, loads);
        Assert.Equal(42, hit.Id);
        Assert.True(JToken.DeepEquals(JToken.Parse("{\"id\":42,\"userIds\":[7]}"),
            JToken.Parse(fixture.Raw(key)!)));

        var nullKey = fixture.Key("null");
        var nullLoads = 0;
        Task<string?> LoadNull() { nullLoads++; return Task.FromResult<string?>(null); }
        Assert.Null(await fixture.Cache.GetOrCreateAsync(nullKey, LoadNull, TimeSpan.FromMinutes(1)));
        Assert.Null(await fixture.Cache.GetOrCreateAsync(nullKey, LoadNull, TimeSpan.FromMinutes(1)));
        Assert.Equal(1, nullLoads);
        Assert.Equal("null", fixture.Raw(nullKey));

        var textKey = fixture.Key("text");
        await fixture.Cache.GetOrCreateAsync(textKey, () => Task.FromResult("null"), TimeSpan.FromMinutes(1));
        Assert.Equal("null", await fixture.Cache.GetOrCreateAsync(textKey,
            () => Task.FromResult("changed"), TimeSpan.FromMinutes(1)));
    }

    [Theory]
    [MemberData(nameof(Backends))]
    public async Task RemovingQueryCacheReloadsPermissions(string? connection)
    {
        using var fixture = new CacheFixture(connection);
        var key = fixture.Key("permissions");
        var permissions = new long[] { 7, 8 };
        Task<Payload> Load() => Task.FromResult(new Payload(42, permissions));

        await fixture.Cache.GetOrCreateAsync(key, Load, TimeSpan.FromMinutes(1));
        permissions = [7];
        await fixture.Cache.RemoveAsync(key);
        Assert.Null(fixture.Raw(key));

        var result = await fixture.Cache.GetOrCreateAsync(key, Load, TimeSpan.FromMinutes(1));
        Assert.Equal(new long[] { 7 }, result.UserIds);
        var cached = await fixture.Cache.GetOrCreateAsync(key, Load, TimeSpan.FromMinutes(1));
        Assert.Equal(new long[] { 7 }, cached.UserIds);
    }

    [Theory]
    [MemberData(nameof(Backends))]
    public async Task AbsoluteExpirationAndSlidingExpirationRetainTheirSemantics(string? connection)
    {
        using var fixture = new CacheFixture(connection);
        var key = fixture.Key("sliding");
        var absolute = fixture.Key("absolute");
        await fixture.Cache.SetAsync(absolute, "fixed", TimeSpan.FromSeconds(2), CacheExpireType.Absolute);
        await fixture.Cache.SetAsync(key, "active", TimeSpan.FromSeconds(2), CacheExpireType.Relative);
        await Task.Delay(1200);
        Assert.Equal("fixed", fixture.Cache.Get<string>(absolute));
        Assert.Equal("active", fixture.Cache.Get<string>(key));
        await Task.Delay(1200);
        Assert.Null(await fixture.Cache.GetAsync<string>(absolute));
        Assert.Equal("active", await fixture.Cache.GetAsync<string>(key));
        await Task.Delay(2400);
        Assert.Null(await fixture.Cache.GetAsync<string>(key));
    }

    [Theory]
    [MemberData(nameof(Backends))]
    public async Task CancelledLoadDoesNotPublishItsResult(string? connection)
    {
        using var fixture = new CacheFixture(connection);
        using var cancellation = new CancellationTokenSource();
        var key = fixture.Key("cancelled");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Cache.GetOrCreateAsync(key, () =>
        {
            cancellation.Cancel();
            return Task.FromResult(new Payload(42, [7]));
        }, TimeSpan.FromMinutes(1), cancellation.Token));
        Assert.Null(fixture.Raw(key));
    }

    [Fact]
    public void SynchronousInterceptorRestoresDeclaredTypeWithoutStoredTypeMetadata()
    {
        using var fixture = new CacheFixture(null);
        using var database = new SqlSugar.SqlSugarScope(new SqlSugar.ConnectionConfig
        {
            ConnectionString = "Data Source=:memory:", DbType = SqlSugar.DbType.Sqlite
        });
        var unitOfWork = new UnitOfWork(database, NullLogger<UnitOfWork>.Instance);
        var target = new CachedReader();
        var proxy = new ProxyGenerator().CreateInterfaceProxyWithTarget<ICachedReader>(target,
            new CacheAop(fixture.Cache, unitOfWork));
        var key = fixture.Key("sync");
        Assert.Equal(42, proxy.Read(key).Id);
        Assert.Equal(new long[] { 7 }, proxy.Read(key).UserIds);
        Assert.Equal(1, target.Loads);
    }

    public interface ICachedReader { Payload Read(string key); }

    public class CachedReader : ICachedReader
    {
        public int Loads { get; private set; }
        [UseCache(Expiration = 1)]
        public Payload Read(string key) { Loads++; return new Payload(42, [7]); }
    }

    public sealed record Payload(long Id, long[] UserIds);

    private sealed class CacheFixture : IDisposable
    {
        private readonly ConnectionMultiplexer? _redis;
        private readonly Microsoft.Extensions.Caching.Memory.MemoryCache? _memory;
        private readonly List<string> _keys = [];
        private readonly string _prefix = "test:cache:" + Guid.NewGuid().ToString("N") + ":";
        public ICache Cache { get; }

        public CacheFixture(string? connection)
        {
            if (connection != null)
            {
                _redis = ConnectionMultiplexer.Connect(connection);
                Cache = new RedisCache(_redis.GetDatabase());
            }
            else
            {
                _memory = new Microsoft.Extensions.Caching.Memory.MemoryCache(Options.Create(new MemoryCacheOptions()));
                Cache = new MemoryCache(_memory);
            }
        }

        public string Key(string suffix) { var key = _prefix + suffix; _keys.Add(key); return key; }
        public string? Raw(string key) => _redis == null ? _memory!.Get<string>(key) : (string?)_redis.GetDatabase().StringGet(key);

        public void Dispose()
        {
            _memory?.Dispose();
            if (_redis == null) return;
            foreach (var key in _keys)
                _redis.GetDatabase().KeyDelete(new RedisKey[] { key, key + ":sliding" });
            _redis.Dispose();
        }
    }
}
