
namespace Ape.Volo.Common.Attributes;

/// <summary>
/// dto映射属性。<br/>
/// 只允许标注在程序集上——契约层（Application.Contracts）不得引用实体，
/// 映射关系因此统一声明在应用层（Application）的 Mapping 目录下。
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AutoMappingAttribute : Attribute
{
    /// <summary>
    /// 初始化 <see cref="AutoMappingAttribute"/> 的新实例。
    /// </summary>
    /// <param name="sourceType">sourceType</param>
    /// <param name="targetType">targetType</param>

    public AutoMappingAttribute(Type sourceType, Type targetType)
    {
        SourceType = sourceType;
        TargetType = targetType;
    }

    /// <summary>
    /// 源对象（实体）
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// 目标对象（模型）
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// 映射方向。<br/>
    /// false（默认）：实体 -&gt; 模型，用于展示类 VO。<br/>
    /// true：模型 -&gt; 实体，用于新增/编辑类 Dto。
    /// </summary>
    public bool ToEntity { get; set; }
}
