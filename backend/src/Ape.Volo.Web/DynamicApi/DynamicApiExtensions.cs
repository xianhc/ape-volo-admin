using Ape.Volo.Core.ConfigOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 动态接口注册与启动日志
/// </summary>
public static class DynamicApiExtensions
{
    /// <summary>
    /// 注册动态接口：契约层接口作为控制器，方法按约定映射为端点
    /// </summary>
    /// <param name="mvc">MVC 构建器</param>
    /// <param name="configuration">配置根对象</param>
    /// <returns>MVC 构建器</returns>
    public static IMvcBuilder AddDynamicApi(this IMvcBuilder mvc, IConfiguration configuration)
    {
        var options = configuration.GetConfigurableOptions<DynamicApiOptions>();
        var contractsAssembly = typeof(IScheduledJobService).Assembly;
        var registry = DynamicApiRegistry.Build(contractsAssembly, options);

        mvc.Services.AddSingleton(registry);
        mvc.Services.AddSingleton<IApplicationModelProvider>(new DynamicApiApplicationModelProvider(registry));
        // 删除接口的正文是 { "ids": [...] }，需要按字段名取集合
        mvc.AddMvcOptions(mvcOptions =>
            mvcOptions.ModelBinderProviders.Insert(0, new DynamicApiBodyCollectionModelBinderProvider()));
        return mvc;
    }

    /// <summary>
    /// 启动时输出已注册的动态接口清单
    /// </summary>
    /// <param name="app">应用实例</param>
    /// <returns>应用实例</returns>
    public static WebApplication UseDynamicApi(this WebApplication app)
    {
        var registry = app.Services.GetRequiredService<DynamicApiRegistry>();
        var endpoints = registry.All.OrderBy(endpoint => endpoint.RouteTemplate, StringComparer.Ordinal).ToList();

        app.Logger.LogInformation("动态接口已注册 {Count} 个端点", endpoints.Count);
        foreach (var endpoint in endpoints)
        {
            // 明细走 Debug：Information 会经 Serilog 写入日志库，启动阶段应尽量少写
            app.Logger.LogDebug("  {Endpoint}  ← {Service}.{Method}", endpoint, endpoint.ServiceType.Name,
                endpoint.Method.Name);
        }

        foreach (var skipped in registry.Skipped)
        {
            app.Logger.LogWarning("  跳过 {Skipped}", skipped);
        }

        return app;
    }
}
