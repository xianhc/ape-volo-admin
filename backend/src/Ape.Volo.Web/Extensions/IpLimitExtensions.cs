using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Serilog;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.ConfigOptions;
using AspNetCoreRateLimit;
using ILogger = Serilog.ILogger;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// IP限流策略中间件
/// </summary>
public static class IpLimitExtensions
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(IpLimitExtensions));

    /// <summary>
    /// 启用 IP 限流中间件，仅当配置 IpLimit 为 true 时生效
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <exception cref="ArgumentNullException">应用构建器为 null</exception>
    public static void UseIpLimitMiddleware(this IApplicationBuilder app)
    {
        if (app.IsNull())
            throw new ArgumentNullException(nameof(app));
        try
        {
            if (app.ApplicationServices.GetRequiredService<IOptions<MiddlewareOptions>>().Value.IpLimit)
            {
                app.UseIpRateLimiting();
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error occured limiting ip rate.\n{e.Message}");
            throw;
        }
    }
}