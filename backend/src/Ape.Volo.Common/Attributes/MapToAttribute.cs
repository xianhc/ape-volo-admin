

namespace Ape.Volo.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MapToAttribute : Attribute
{
    /// <summary>
    /// 初始化 <see cref="MapToAttribute"/> 的新实例。
    /// </summary>
    /// <param name="targetType">targetType</param>

    public MapToAttribute(Type targetType)
    {
        TargetType = targetType;
    }

    public Type TargetType { get; }
}
