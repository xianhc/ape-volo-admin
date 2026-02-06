using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 运行模式
/// </summary>
[OptionsSettings]
public class RunModeOptions
{
    /// <summary>
    /// 运行模式
    /// </summary>
    public RunMode RunMode { get; set; }
}