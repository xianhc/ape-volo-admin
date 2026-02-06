using System.Collections.Generic;
using Ape.Volo.Common.MultiLanguage.Contract;
using Ape.Volo.Common.MultiLanguage.Resources;
using Microsoft.Extensions.Localization;

namespace Ape.Volo.Common.MultiLanguage;

public class LocalizationService : ILocalizationService
{
    // 用于多语言文本查找的本地化器实例
    private readonly IStringLocalizer _localizer;

    /// <summary>
    /// 构造函数，依赖注入 IStringLocalizer<Language/> 用于处理多语言资源。
    /// </summary>
    /// <param name="localizer">IStringLocalizer 实例，用于多语言资源查找。</param>
    public LocalizationService(IStringLocalizer<Language> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// 获取指定名称的本地化字符串。
    /// </summary>
    /// <param name="name">资源的名称键。</param>
    /// <returns>对应的本地化字符串。</returns>
    public string R(string name)
    {
        return _localizer[name];
    }

    /// <summary>
    /// 获取格式化参数后的本地化字符串。
    /// </summary>
    /// <param name="name">资源名称键。</param>
    /// <param name="arguments">格式化参数。</param>
    /// <returns>格式化后的本地化字符串。</returns>
    public string R(string name, params object[] arguments)
    {
        return _localizer[name, arguments];
    }

    /// <summary>
    /// 获取所有可用的本地化字符串。
    /// </summary>
    /// <param name="includeParentCultures">是否包含父文化的信息。</param>
    /// <returns>所有本地化字符串集合。</returns>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        // 注意：此处未使用 includeParentCultures 参数，实际如需支持应传递参数
        return _localizer.GetAllStrings();
    }
}