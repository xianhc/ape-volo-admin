using Ape.Volo.Web.Extensions;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Xunit;

namespace Ape.Volo.Web.Tests;

public class IpRateLimitTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("", 1)]
    [InlineData("test-password", 1)]
    [InlineData("test-password", 4)]
    public void RateLimitUsesConfiguredRedisDatabaseRegardlessOfPassword(string password, int database)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["System:UseRedisCache"] = "true",
            ["Redis:Host"] = "localhost",
            ["Redis:Port"] = "6379",
            ["Redis:Password"] = password,
            ["Redis:Index"] = database.ToString()
        }).Build();
        var services = new ServiceCollection();
        services.AddIpStrategyRateLimitService(configuration);
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
        var connection = options.ConfigurationOptions ?? ConfigurationOptions.Parse(options.Configuration!);

        Assert.Equal(database, connection.DefaultDatabase ?? 0);
    }
}
