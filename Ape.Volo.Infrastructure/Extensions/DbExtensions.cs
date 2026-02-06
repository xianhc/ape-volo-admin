using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.SeedData;
using Microsoft.Extensions.DependencyInjection;

namespace Ape.Volo.Infrastructure.Extensions;

/// <summary>
/// 数据库上下文扩展配置
/// </summary>
public static class DbExtensions
{
    public static void AddDbService(this IServiceCollection services)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));

        services.AddScoped<SeedService>();
        services.AddScoped<DataContext>();
    }
}