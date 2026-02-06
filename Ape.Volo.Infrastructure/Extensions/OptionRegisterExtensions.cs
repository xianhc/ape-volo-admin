using System.Reflection;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Global;
using Ape.Volo.Core.ConfigOptions.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Ape.Volo.Infrastructure.Extensions;

/// <summary>
/// 可配置选项扩展
/// </summary>
public static class OptionRegisterExtensions
{
    /// <summary>
    /// 注册配置选项
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void AddOptionRegisterService(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        var optionTypes = GlobalType.CoreTypes
            .Where(x => x.GetCustomAttribute<OptionsSettingsAttribute>() != null).ToList();

        foreach (var optionType in optionTypes)
        {
            services.AddConfigurableOptions(optionType);
        }
    }
}