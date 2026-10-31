using System.Reflection;
using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 可配置选项扩展
/// </summary>
public static class OptionRegisterExtensions
{
    /// <summary>
    /// 注册配置选项
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置根对象</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void AddOptionRegisterService(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var optionTypes = typeof(Ape.Volo.Core.ConfigOptions.SystemOptions).Assembly.GetExportedTypes()
            .Where(x => x.GetCustomAttribute<OptionsSettingsAttribute>() != null).ToList();

        foreach (var optionType in optionTypes)
        {
            services.AddConfigurableOptions(optionType, configuration);
        }
    }
}