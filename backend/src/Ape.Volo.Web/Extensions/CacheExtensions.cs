using Ape.Volo.Common.Extensions;
using Ape.Volo.Application.Contracts.Abstractions.Caching;
using Ape.Volo.Core.Caches.Distributed;
using Ape.Volo.Core.Caches.Redis;
using Ape.Volo.Core.ConfigOptions;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 缓存扩展配置
/// </summary>
public static class CacheExtensions
{
    /// <summary>
    /// 按配置注册缓存实现：启用 Redis 时使用 <see cref="RedisCache"/>，否则使用进程内缓存
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置根对象</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddCacheService(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.IsNull())
            throw new ArgumentNullException(nameof(services));
        services.AddDistributedMemoryCache(); //session需要

        if (configuration.GetConfigurableOptions<SystemOptions>().UseRedisCache)
        {
            services.AddSingleton<RedisCache>();
            services.AddSingleton<ICache>(provider => provider.GetRequiredService<RedisCache>());
            services.AddSingleton<ICacheKeyCatalog>(provider => provider.GetRequiredService<RedisCache>());
            return;
        }

        services.AddSingleton<Microsoft.Extensions.Caching.Memory.MemoryCache>();
        services.AddSingleton<ICache, DistributedCache>();
    }
}
