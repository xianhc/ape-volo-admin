using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.IdGeneration;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 雪花 ID 生成器的配置与注册。
/// </summary>
public static class SnowflakeIdExtensions
{
    /// <summary>
    /// 启动时校验配置并注册单例，进程内所有调用共享同一生成序列。
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">宿主配置</param>
    public static IServiceCollection AddSnowflakeIdService(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetConfigurableOptions<SnowflakeIdOptions>();
        services.AddSingleton<IIdGenerator>(new SnowflakeIdGenerator(options));
        return services;
    }
}
