using Ape.Volo.Common.Extensions;
using Ape.Volo.Core;
using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;
using Microsoft.Extensions.DependencyInjection;

namespace Ape.Volo.Infrastructure.Extensions
{
    /// <summary>
    /// IP地理位置查询扩展
    /// </summary>
    public static class IpSearcherExtensions
    {
        public static void AddIpSearcherService(this IServiceCollection services)
        {
            if (services.IsNull()) throw new ArgumentNullException(nameof(services));
            services.AddSingleton<ISearcher>(new Searcher(CachePolicy.Content,
                Path.Combine(App.WebHostEnvironment.WebRootPath, "resources", "ip", "ip2region.xdb")));
        }
    }
}