using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.ConfigOptions;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 跨域扩展配置
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// 按配置注册跨域策略，支持放行全部来源或指定来源
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置根对象</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddCorsService(this IServiceCollection services, IConfiguration configuration)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));

        var options = configuration.GetConfigurableOptions<CorsOptions>();
        services.AddCors(c =>
        {
            if (options.EnableAll)
            {
                //允许任意跨域请求
                c.AddPolicy(options.Name,
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("X-Request-ID");
                    });
            }
            else
            {
                c.AddPolicy(options.Name,
                    policy =>
                    {
                        policy
                            .WithOrigins(options.Policy.Select(x => x.Domain).ToArray())
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials()
                            .WithExposedHeaders("X-Request-ID");
                    });
            }
        });
    }
}
