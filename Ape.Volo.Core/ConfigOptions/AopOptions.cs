using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// Aop配置
/// </summary>
[OptionsSettings]
public class AopOptions
{
    /// <summary>
    /// 事务
    /// </summary>
    public bool Transactions { get; set; }

    /// <summary>
    /// 缓存
    /// </summary>
    public bool Cache { get; set; }
}