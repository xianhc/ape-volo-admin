using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Helper.Serilog;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.SeedData;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Ape.Volo.Infrastructure.Middleware;

public static class DataSeederMiddleware
{
    private static readonly ILogger Logger = SerilogManager.GetLogger(typeof(DataSeederMiddleware));

    public static void UseDataSeederMiddleware(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        try
        {
            var systemOptions = App.GetOptions<SystemOptions>();
            var tenantOptions = App.GetOptions<TenantOptions>();
            if (systemOptions.IsInitDb)
            {
                var dataContext = app.ApplicationServices.GetRequiredService<DataContext>();
                // 优先初始化日志库
                SeedService.InitLogDataAsync(dataContext, systemOptions.LogDataBase).Wait();
                Thread.Sleep(500); //保证顺序输出
                SeedService.InitMasterDataAsync(dataContext, systemOptions, tenantOptions).Wait();
                if (tenantOptions.Enabled && tenantOptions.Type == TenantType.Db)
                {
                    Thread.Sleep(500);
                    SeedService.InitTenantDataAsync(dataContext).Wait();
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Error when creating database initialization data:\n{e.Message}");
            throw;
        }
    }
}