using Ape.Volo.Shared.Enums;

namespace Ape.Volo.Shared.Attributes;

/// <summary>
/// 声明查询条件：标注在查询参数属性上，由基础设施层翻译成实际的查询表达式。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class QueryConditionAttribute : Attribute
{
    /// <summary>
    /// 条件类型
    /// </summary>
    public QueryConditionType ConditionType { get; set; }

    /// <summary>
    /// 指定字段名称 请求参数与实体参数名称不一致时使用
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// 指定字段名称,一个请求参数对应对应实体字段时使用
    /// </summary>
    public string[] FieldNameItems { get; set; } = [];

    /// <summary>
    /// 是否大于数字默认值才添加条件模型 例如int long默认0根据情况判断
    /// </summary>
    public bool IsGreaterThanNumberDefault { get; set; } = true;
}
