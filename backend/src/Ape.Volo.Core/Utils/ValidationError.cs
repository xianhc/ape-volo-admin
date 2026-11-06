using System.Linq.Expressions;
using System.Reflection;
using Ape.Volo.Common.Reflection;

namespace Ape.Volo.Core.Utils;

/// <summary>
/// 根据实体和字段说明生成业务校验提示
/// </summary>
public static class ValidationError
{
    /// <summary>
    /// 根据字段表达式生成重复数据提示
    /// </summary>
    /// <param name="field">DTO 或实体的字段访问表达式</param>
    /// <param name="entityType">服务对应的实体类型</param>
    /// <returns>字段重复提示</returns>
    public static string IsExist<T>(Expression<Func<T>> field, Type? entityType = null)
    {
        var (modelType, propertyName) = GetField(field);
        var name = ModelDescription.GetName(entityType ?? modelType, propertyName, modelType);
        return $"{name}“{field.Compile().Invoke()}”已存在。";
    }

    /// <summary>
    /// 根据实体类型和可选标识生成数据不存在提示
    /// </summary>
    /// <param name="value">记录标识或查询字段的值</param>
    /// <param name="propertyName">查询字段，默认主键</param>
    /// <returns>数据不存在提示</returns>
    public static string NotExist<TEntity>(object? value = null, string propertyName = "Id")
    {
        var name = ModelDescription.GetName(typeof(TEntity));
        return value == null ? $"{name}不存在。"
            : $"{name}（{ModelDescription.GetName(typeof(TEntity), propertyName)}：{value}）不存在。";
    }

    /// <summary>
    /// 生成存在数据关联的提示
    /// </summary>
    /// <returns>数据关联提示</returns>
    public static string DataAssociationExists() => "存在数据关联，请解除后再试。";

    /// <summary>
    /// 根据字段表达式生成必填提示
    /// </summary>
    /// <param name="field">DTO 或实体的字段访问表达式</param>
    /// <param name="entityType">服务对应的实体类型</param>
    /// <returns>字段必填提示</returns>
    public static string Required<T>(Expression<Func<T>> field, Type? entityType = null)
    {
        var (modelType, propertyName) = GetField(field);
        return $"{ModelDescription.GetName(entityType ?? modelType, propertyName, modelType)}不能为空。";
    }

    private static (Type ModelType, string PropertyName) GetField(LambdaExpression field)
    {
        ArgumentNullException.ThrowIfNull(field);
        var body = field.Body is UnaryExpression unary ? unary.Operand : field.Body;
        if (body is not MemberExpression { Member: PropertyInfo property })
            throw new ArgumentException("请使用 DTO 或实体的属性访问表达式。", nameof(field));
        return (property.DeclaringType!, property.Name);
    }
}
