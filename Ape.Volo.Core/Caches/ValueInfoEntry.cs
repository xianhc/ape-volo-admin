using Ape.Volo.Common.Enums;

namespace Ape.Volo.Core.Caches;

/// <summary>
/// 缓存对象
/// </summary>
public class ValueInfoEntry
{
    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 命名空间
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public TimeSpan ExpireTime { get; set; }

    /// <summary>
    /// 缓存类型
    /// </summary>
    public CacheExpireType ExpireType { get; set; }
}