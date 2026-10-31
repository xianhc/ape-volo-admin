using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Serilog;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.ConfigOptions;
using StackExchange.Profiling;
using Ape.Volo.Application.Contracts.Authorization;
using Microsoft.Extensions.Hosting;
using ILogger = Serilog.ILogger;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// MiniProfiler性能监控启动器
/// </summary>
public static class MiniProfilerExtensions
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(MiniProfilerExtensions));

    /// <summary>
    /// 注册 MiniProfiler 性能监控服务，路由前缀固定为 /profiler
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddMiniProfilerService(this IServiceCollection services)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));

        services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                //(options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(10);
                options.PopupRenderPosition = RenderPosition.Left;
                options.PopupShowTimeWithChildren = true;

                options.ResultsAuthorizeAsync = CanViewResultsAsync;
                options.ResultsListAuthorizeAsync = CanViewResultsAsync;
            }
        );
    }

    private static async Task<bool> CanViewResultsAsync(HttpRequest request)
    {
        if (request.HttpContext.User.Identity?.IsAuthenticated != true) return false;
        var services = request.HttpContext.RequestServices;
        var user = services.GetRequiredService<ICurrentUser>();
        var roles = await services.GetRequiredService<IPermissionService>()
            .GetRoleCodesAsync(user.Id, request.HttpContext.RequestAborted);
        return roles.Contains(RoleCodes.Admin);
    }

    /// <summary>
    /// 按配置启用性能监控
    /// </summary>
    public static void UseMiniProfilerMiddleware(this IApplicationBuilder app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));
        try
        {
            if (app.ApplicationServices.GetRequiredService<IHostEnvironment>().IsDevelopment() &&
                app.ApplicationServices.GetRequiredService<IOptions<MiddlewareOptions>>().Value.MiniProfiler)
            {
                // 性能分析
                app.UseMiniProfiler();
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Performance monitoring startup error:/n{e.Message}");
            throw;
        }
    }
}
