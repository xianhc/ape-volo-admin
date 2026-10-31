using Ape.Volo.Core.Mapping;
using Ape.Volo.Application.Mapping;
using Mapster;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 将映射规则和映射器绑定到当前宿主，不修改进程共享的 Mapster 配置。
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// 注册宿主独立的映射配置，并在首次使用前应用所有 IRegister 规则。
    /// </summary>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddMappingService(this IServiceCollection services)
    {
        services.AddSingleton<IRegister, AutoMappingRegister>();
        services.AddSingleton(provider =>
        {
            var config = new TypeAdapterConfig();
            config.Apply(provider.GetServices<IRegister>());
            return config;
        });
        services.AddSingleton<IMapper, Mapper>();
        return services;
    }
}
