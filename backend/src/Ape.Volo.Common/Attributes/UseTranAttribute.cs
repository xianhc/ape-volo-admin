using Ape.Volo.Common.Enums;

namespace Ape.Volo.Common.Attributes;

/// <summary>
/// 事务特性 AOP拦截使用
/// </summary>
/// <remarks>
/// 默认传播行为为 <see cref="Propagation.Required"/>；
/// 也可写 <c>[UseTran(Propagation.Mandatory)]</c> 或 <c>[UseTran(Propagation = Propagation.Nested)]</c>。
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class UseTranAttribute : Attribute
{
    /// <summary>
    /// 初始化 <see cref="UseTranAttribute"/> 的新实例。
    /// </summary>
    public UseTranAttribute()
    {
    }

    /// <summary>
    /// 初始化 <see cref="UseTranAttribute"/> 的新实例。
    /// </summary>
    /// <param name="propagation">事务传播行为</param>
    public UseTranAttribute(Propagation propagation = Propagation.Required)
    {
        Propagation = propagation;
    }

    /// <summary>
    /// 事务传播行为，默认 <see cref="Propagation.Required"/>。
    /// </summary>
    public Propagation Propagation { get; set; } = Propagation.Required;
}
