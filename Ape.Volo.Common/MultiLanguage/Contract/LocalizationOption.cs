using System;

namespace Ape.Volo.Common.MultiLanguage.Contract;

/// <summary>
/// 表示多语言（本地化）配置选项，
/// 用于设置多语言资源、默认语言及支持的文化信息。
/// </summary>
public class LocalizationOption
{
    /// <summary>
    /// 多语言资源文件对应的类型。通常用于定位资源所在的程序集或类。
    /// </summary>
    public Type LocalizationType { get; set; }

    /// <summary>
    /// 资源文件的路径，例如 "Resources/Localization"。
    /// </summary>
    public string ResourcesPath { get; set; }

    /// <summary>
    /// 默认的区域文化（如 "zh-CN"、"en-US" 等）。
    /// </summary>
    public string DefaultCulture { get; set; }

    /// <summary>
    /// 支持的区域文化列表（如 ["zh-CN", "en-US"]）。
    /// </summary>
    public string[] SupportedCultures { get; set; }
}