using Ape.Volo.Common.Extensions;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.DependencyInjection;

namespace Ape.Volo.Infrastructure.Extensions;

/// <summary>
/// 跨域扩展配置
/// </summary>
public static class CorsExtensions
{
    public static void AddCorsService(this IServiceCollection services)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));

        var options = App.GetOptions<CorsOptions>();
        services.AddCors(c =>
        {
            if (options.EnableAll)
            {
                //允许任意跨域请求
                c.AddPolicy(options.Name,
                    policy =>
                    {
                        policy
                            .SetIsOriginAllowed(host => true)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
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
                            .AllowAnyMethod();
                    });
            }
        });
    }
}