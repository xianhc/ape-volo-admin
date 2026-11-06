using Ape.Volo.Common.Extensions;
using Ape.Volo.Web.Services;

namespace Ape.Volo.Web.Extensions
{
    /// <summary>
    /// IP地理位置查询扩展
    /// </summary>
    public static class IpSearcherExtensions
    {
        /// <summary>
        /// 注册基于 ip2region 离线库的 IP 归属地查询服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <exception cref="ArgumentNullException">服务集合为 null</exception>
        public static void AddIpSearcherService(this IServiceCollection services)
        {
            if (services.IsNull()) throw new ArgumentNullException(nameof(services));
            services.AddSingleton<IIpLocationService>(provider =>
            {
                var directory = Path.Combine(provider.GetRequiredService<IWebHostEnvironment>().ContentRootPath,
                    "resources", "ip");
                return new IpLocationService(Path.Combine(directory, "ip2region_v4.xdb"),
                    Path.Combine(directory, "ip2region_v6.xdb"),
                    provider.GetRequiredService<ILogger<IpLocationService>>());
            });
        }
    }
}
