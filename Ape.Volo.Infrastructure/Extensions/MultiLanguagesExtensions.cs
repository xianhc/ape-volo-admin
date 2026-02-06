using System.Globalization;
using Ape.Volo.Common.MultiLanguage;
using Ape.Volo.Common.MultiLanguage.Contract;
using Ape.Volo.Common.MultiLanguage.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Ape.Volo.Infrastructure.Extensions;

/// <summary>
/// 多语言扩展配置
/// </summary>
public static class MultiLanguagesExtensions
{
    /// <summary>
    /// 多语言支持
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomMultiLanguagesService(this IServiceCollection services,
        Action<LocalizationOption> configureOptions = null)
    {
        // 配置自定义本地化选项
        var localizationOption = new LocalizationOption
        {
            LocalizationType = typeof(Language),
            ResourcesPath = "", // 默认资源路径
            DefaultCulture = "zh-CN", // 默认语言
            SupportedCultures = ["zh-CN", "en-US"] // 默认支持的语言 默认支持的语言
        };
        configureOptions?.Invoke(localizationOption);

        // 配置资源路径
        services.AddLocalization(options => options.ResourcesPath = localizationOption.ResourcesPath);

        // 支持的语言文化
        var supportedCultures =
            Array.ConvertAll(localizationOption.SupportedCultures, culture => new CultureInfo(culture));

        // 配置请求本地化选项
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(localizationOption.DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        if (localizationOption.LocalizationType != null)
        {
            services.AddSingleton(localizationOption);
        }

        services.AddSingleton<ILocalizationService, LocalizationService>();

        return services;
    }

    /// <summary>
    /// 添加数据注解本地化支持
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="languageType"></param>
    /// <returns></returns>
    public static IMvcBuilder AddDataAnnotationsLocalization(this IMvcBuilder builder, Type languageType = null)
    {
        languageType ??= typeof(Language);

        builder.AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(languageType);
        });

        return builder;
    }

    public static IApplicationBuilder UseCustomRequestLocalization(this IApplicationBuilder builder,
        Action<LocalizationOption> configureOptions = null)
    {
        // 配置自定义本地化选项
        var localizationOption = new LocalizationOption
        {
            LocalizationType = typeof(Language),
            ResourcesPath = "", // 默认资源路径
            DefaultCulture = "zh-CN", // 默认语言
            SupportedCultures = ["zh-CN", "en-US"] // 默认支持的语言
        };
        configureOptions?.Invoke(localizationOption);

        var supportedCultures = localizationOption.SupportedCultures
            .Select(culture => new CultureInfo(culture))
            .ToList();

        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture =
                new RequestCulture(localizationOption.DefaultCulture),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
        };

        options.RequestCultureProviders = new List<IRequestCultureProvider>
        {
            new QueryStringRequestCultureProvider(), // 优先使用 URL 参数
            new AcceptLanguageHeaderRequestCultureProvider(), // 然后使用浏览器的 Accept-Language
            new CookieRequestCultureProvider() // 最后使用 Cookie
        };

        builder.UseRequestLocalization(options);

        return builder;
    }
}