using System.Reflection;
using Ape.Volo.Common.Attributes;
using Microsoft.Extensions.Options;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 宿主配置绑定与选项注册。
/// </summary>
public static class ConfigurableOptions
{
    /// <summary>
    /// 在容器构建前按与选项注册相同的配置路径读取选项。
    /// </summary>
    public static TOptions GetConfigurableOptions<TOptions>(this IConfiguration configuration)
        where TOptions : class, new()
    {
        return configuration.GetSection(GetConfigurationPath(typeof(TOptions))).Get<TOptions>() ?? new TOptions();
    }

    /// <summary>
    /// 按配置节绑定强类型选项
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="type">选项类型</param>
    /// <param name="configuration">配置根对象（由宿主传入，不依赖静态定位器）</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddConfigurableOptions(this IServiceCollection services, Type type,
        IConfiguration configuration)
    {
        string path = GetConfigurationPath(type);
        var config = configuration.GetSection(path);

        Type iOptionsChangeTokenSource = typeof(IOptionsChangeTokenSource<>);
        Type iConfigureOptions = typeof(IConfigureOptions<>);
        Type configurationChangeTokenSource = typeof(ConfigurationChangeTokenSource<>);
        Type namedConfigureFromConfigurationOptions = typeof(NamedConfigureFromConfigurationOptions<>);
        iOptionsChangeTokenSource = iOptionsChangeTokenSource.MakeGenericType(type);
        iConfigureOptions = iConfigureOptions.MakeGenericType(type);
        configurationChangeTokenSource = configurationChangeTokenSource.MakeGenericType(type);
        namedConfigureFromConfigurationOptions = namedConfigureFromConfigurationOptions.MakeGenericType(type);

        services.AddOptions();
        services.AddSingleton(iOptionsChangeTokenSource,
            Activator.CreateInstance(configurationChangeTokenSource, Options.DefaultName, config) ??
            throw new InvalidOperationException());
        return services.AddSingleton(iConfigureOptions,
            Activator.CreateInstance(namedConfigureFromConfigurationOptions, Options.DefaultName, config) ??
            throw new InvalidOperationException());
    }

    /// <summary>
    /// 获取配置路径
    /// </summary>
    /// <param name="optionsType">选项类型</param>
    /// <returns></returns>
    public static string GetConfigurationPath(Type optionsType)
    {
        var optionsSettings = optionsType.GetCustomAttribute<OptionsSettingsAttribute>(false);

        // Default suffix to be removed
        var defaultSuffix = nameof(Options);

        // Determine the configuration path
        string path = optionsSettings switch
        {
            // If there's no [OptionsSettings] attribute, remove the "Options" suffix if present
            null => optionsType.Name.EndsWith(defaultSuffix)
                ? optionsType.Name.Substring(0, optionsType.Name.Length - defaultSuffix.Length)
                : optionsType.Name,

            // If [OptionsSettings] attribute exists, use its Path if specified, otherwise use the class name
            _ => !string.IsNullOrWhiteSpace(optionsSettings.Path)
                ? optionsSettings.Path
                : optionsType.Name.EndsWith(defaultSuffix)
                    ? optionsType.Name.Substring(0, optionsType.Name.Length - defaultSuffix.Length)
                    : optionsType.Name
        };

        return path;
    }
}
