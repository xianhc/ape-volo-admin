using Ape.Volo.Core.Serilog;
using Microsoft.Extensions.Options;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.SeedData;
using ILogger = Serilog.ILogger;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 初始化数据扩展
/// </summary>
public static class DataSeederExtensions
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(DataSeederExtensions));

    /// <summary>
    /// 启用时先初始化日志库再初始化主库数据，仅当配置 IsInitDb 为 true 时执行
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <exception cref="ArgumentNullException">应用构建器为 null</exception>
    public static async Task UseDataSeederMiddlewareAsync(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        try
        {
            var systemOptions = app.ApplicationServices.GetRequiredService<IOptions<SystemOptions>>().Value;
            if (systemOptions.IsInitDb)
            {
                await using var scope = app.ApplicationServices.CreateAsyncScope();
                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                // 优先初始化日志库：await 保证顺序，不再阻塞启动线程
                await SeedService.InitLogDataAsync(dataContext,
                    scope.ServiceProvider.GetRequiredService<IOptions<LogDataConnectionOptions>>().Value.ConnId);
                await SeedService.InitMasterDataAsync(dataContext, systemOptions,
                    app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error when creating database initialization data:\n{e.Message}");
            throw;
        }
    }
}
