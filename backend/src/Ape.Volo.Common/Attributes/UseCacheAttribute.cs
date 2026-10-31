using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Common.Attributes;

/// <summary>
/// 缓存特性，由 AOP 对当前缓存实现执行读写。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class UseCacheAttribute : Attribute
{
    /// <summary>
    /// 过期时间，单位为分钟；默认 20 分钟。
    /// </summary>
    public int Expiration { get; set; } = 20;

    /// <summary>
    /// 缓存key前缀
    /// </summary>
    public string KeyPrefix { get; set; } = "";

    /// <summary>
    /// 缓存类型（默认绝对过期）
    /// </summary>
    public CacheExpireType CacheExpireType { get; set; } = CacheExpireType.Absolute;
}
