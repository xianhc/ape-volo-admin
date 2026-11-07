using Ape.Volo.Application.Contracts.Abstractions.Caching;
using Ape.Volo.Application.Contracts.Identity.Auth;
using Ape.Volo.Application.Identity.Auth;
using Ape.Volo.Web.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using MemoryCache = Ape.Volo.Core.Caches.Distributed.DistributedCache;

namespace Ape.Volo.Web.Tests;

public class AuthSessionCacheTests
{
    [Fact]
    public async Task RefreshHashIsConsumedOnceAndRevocationPreventsReuse()
    {
        using var memory = new Microsoft.Extensions.Caching.Memory.MemoryCache(Options.Create(new MemoryCacheOptions()));
        ICache cache = new MemoryCache(memory);
        var service = new AuthSessionService(cache, TimeProvider.System, []);
        var session = new AuthSession
        {
            Id = "session-a", UserId = 42, RefreshTokenHash = "old", AccessTokenId = "access-old",
            LoginInfoJson = "{\"account\":\"test\"}", CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        };
        Assert.True(await service.CreateAsync(session));
        Assert.Equal(new[] { AuthSessionService.SessionKeyPrefix + session.Id },
            await ((ICacheKeyCatalog)cache).GetKeysAsync(""));
        var active = await service.GetActiveAsync(session.Id);
        Assert.NotNull(active);
        Assert.Equal(session.LoginInfoJson, active.LoginInfoJson);

        var rotated = await Task.WhenAll(
            service.RotateAsync(session.Id, "old", "next-a", "access-a"),
            service.RotateAsync(session.Id, "old", "next-b", "access-b"));
        Assert.Single(rotated.Where(success => success));
        await service.RevokeAsync(session.Id);
        Assert.Null(await service.GetActiveAsync(session.Id));
        Assert.False(await service.RotateAsync(session.Id, "next-a", "again", "access-again"));
    }

    [Fact]
    public async Task RevokingUserDeletesTheirSessionsAndAllowsNewLogin()
    {
        var services = new ServiceCollection();
        services.AddCacheService(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ICache>();
        var service = new AuthSessionService(cache, TimeProvider.System, []);
        AuthSession Session(string id) => new()
        {
            Id = id, UserId = 42, RefreshTokenHash = "hash", CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        };
        var first = Session("first");
        Assert.True(await service.CreateAsync(first));
        Assert.True(await service.CreateAsync(Session("second")));
        var other = Session("other");
        other.UserId = 43;
        Assert.True(await service.CreateAsync(other));
        await service.RevokeUserAsync(42);
        Assert.Null(await service.GetActiveAsync("first"));
        Assert.Null(await service.GetActiveAsync("second"));
        Assert.NotNull(await service.GetActiveAsync("other"));
        Assert.Null(await cache.GetAsync<string>(AuthSessionService.SessionKeyPrefix + "first"));
        Assert.Null(await cache.GetAsync<string>(AuthSessionService.SessionKeyPrefix + "second"));
        var next = Session("next");
        Assert.True(await service.CreateAsync(next));
        Assert.NotNull(await service.GetActiveAsync("next"));
    }
}
