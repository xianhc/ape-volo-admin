using Ape.Volo.Core.ConfigOptions;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Redis;
using System.Net;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// IP限流扩展配置
/// </summary>
public static class IpRateLimitExtensions
{
    /// <summary>
    /// 注册 IP 限流策略与计数器存储，启用 Redis 时计数器落 Redis
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置根对象</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddIpStrategyRateLimitService(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        // 限流只使用可信代理中间件解析后的 RemoteIpAddress，不接受客户端自报身份。
        services.PostConfigure<IpRateLimitOptions>(options =>
        {
            options.RealIpHeader = string.Empty;
            options.ClientIdHeader = string.Empty;
            options.ClientWhitelist = [];
        });
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            var proxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
            if (proxies.Length == 0) return; // 沿用框架默认的回环代理信任范围。
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (var proxy in proxies)
            {
                if (!IPAddress.TryParse(proxy, out var address))
                    throw new InvalidOperationException("ForwardedHeaders:KnownProxies 必须包含有效的 IP 地址。");
                options.KnownProxies.Add(address);
            }
        });

        if (configuration.GetConfigurableOptions<SystemOptions>().UseRedisCache)
        {
            var redisOptions = configuration.GetConfigurableOptions<RedisOptions>();
            services.AddStackExchangeRedisCache(option =>
            {
                option.ConfigurationOptions = new ConfigurationOptions
                {
                    EndPoints = { { redisOptions.Host, redisOptions.Port } },
                    DefaultDatabase = redisOptions.Index
                };
                if (!string.IsNullOrWhiteSpace(redisOptions.Password))
                    option.ConfigurationOptions.Password = redisOptions.Password;

                option.InstanceName = "rateLimit:";
            });
            services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
        }
        else
        {
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        }

        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    }
}
