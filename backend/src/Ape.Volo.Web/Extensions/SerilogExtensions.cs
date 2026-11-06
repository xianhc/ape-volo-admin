using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Application.Contracts.Abstractions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ape.Volo.Core.ConfigOptions;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.Serilog;
using Ape.Volo.Web.Logging;
using Serilog;
using Serilog.Events;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// Serilog日志 替换内置Logging
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// 用 Serilog 替换内置日志实现
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddSerilogService(this IServiceCollection services)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });
        //services.AddSingleton<Serilog.Extensions.Hosting.DiagnosticContext>();
    }

    /// <summary>
    /// 按配置装配 Serilog 输出端（文件 / 控制台 / 数据库 / Elasticsearch）
    /// </summary>
    public static IHostBuilder UseSerilogMiddleware(this IHostBuilder builder)
    {
        builder.ConfigureServices((_, services) =>
        {
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton<IFallbackLogWriter, FallbackLogWriter>();
        });
        return builder.UseSerilog((context, services, logger) => //注册Serilog
        {
            //如要想使用setting配置方式，打开下面行注释。注释后面代码即可
            //logger.ReadFrom.Configuration(context.Configuration);
            logger.Enrich.FromLogContext();
            //logger.Enrich.WithProperty("UserName", "system");
            logger.MinimumLevel.Debug(); //日志记录起始等级
            logger.MinimumLevel.Override("Microsoft", LogEventLevel.Information);
            logger.MinimumLevel.Override("Default", LogEventLevel.Information);
            logger.MinimumLevel.Override("System", LogEventLevel.Information);


            var serilogOptions = services.GetRequiredService<IOptions<SerilogOptions>>().Value;
            if (serilogOptions.ToFile)
            {
                logger.WriteToFile(context.HostingEnvironment.ContentRootPath);
            }

            if (services.GetRequiredService<IOptions<SystemOptions>>().Value.RunMode == RunMode.Dev && serilogOptions.ToConsole)
            {
                //输出到控制台
                logger.WriteToConsole();
            }


            if (serilogOptions.ToDb)
            {
                //输出到数据库
                logger.WriteToDb(services.GetRequiredService<IServiceScopeFactory>(),
                    services.GetRequiredService<IOptions<LogDataConnectionOptions>>().Value,
                    services.GetRequiredService<IFallbackLogWriter>(), services.GetRequiredService<IIdGenerator>());
            }

            if (serilogOptions.ToElasticsearch && services.GetRequiredService<IOptions<MiddlewareOptions>>().Value.Elasticsearch)
            {
                //输出到Elasticsearch
                // 官方 sink 使用 ECS 数据流，要求 Elasticsearch 8.15 及以上。
                logger.WriteToElasticsearch(serilogOptions);
            }
        });
    }
}
