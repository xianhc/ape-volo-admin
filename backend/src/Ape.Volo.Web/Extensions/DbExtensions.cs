using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.SeedData;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 数据库上下文扩展配置
/// </summary>
public static class DbExtensions
{
    /// <summary>
    /// 注册数据上下文与种子数据服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <exception cref="ArgumentNullException">服务集合为 null</exception>
    public static void AddDbService(this IServiceCollection services)
    {
        if (services.IsNull()) throw new ArgumentNullException(nameof(services));

        services.AddScoped<SeedService>();
        services.AddScoped<DataContext>();
    }
}