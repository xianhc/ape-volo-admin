using Ape.Volo.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Ape.Volo.Infrastructure.Extensions;

/// <summary>
/// Serilog日志 替换内置Logging
/// </summary>
public static class SerilogExtensions
{
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
}