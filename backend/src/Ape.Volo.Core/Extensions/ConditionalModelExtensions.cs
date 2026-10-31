using System.Reflection;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Attributes;
using Ape.Volo.Shared.Enums;
using SqlSugar;

namespace Ape.Volo.Core.Extensions;

/// <summary>
/// 查询条件模型扩展
/// </summary>
public static class ConditionalModelExtensions
{
    /// <summary>
    /// 应用条件
    /// </summary>
    /// <param name="criteria"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<IConditionalModel> ApplyQueryConditionalModel<T>(this T criteria)
    {
        var conModels = new List<IConditionalModel>();
        if (criteria is null) return conModels;

        var propertyInfos = criteria.GetType().GetProperties();
        foreach (var propertyInfo in propertyInfos)
        {
            var value = propertyInfo.GetValue(criteria);
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                continue;

            var queryConditionAttribute = propertyInfo.GetCustomAttribute<QueryConditionAttribute>();
            if (queryConditionAttribute == null)
                continue;
            if (propertyInfo.PropertyType.IsEnum ||
                Nullable.GetUnderlyingType(propertyInfo.PropertyType)?.IsEnum == true)
            {
                value = (int)value;
            }

            if (queryConditionAttribute.IsGreaterThanNumberDefault)
            {
                if ((value is int || value is long) && Convert.ToInt64(value) <= 0)
                    continue;
            }

            conModels.AddRange(GenerateConditionalModel(queryConditionAttribute, propertyInfo, value));
        }

        return conModels;
    }

    /// <summary>
    /// 生成条件模型
    /// </summary>
    /// <param name="queryConditionAttribute"></param>
    /// <param name="propertyInfo"></param>
    /// <param name="fieldValue"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static List<IConditionalModel> GenerateConditionalModel(QueryConditionAttribute queryConditionAttribute,
        PropertyInfo propertyInfo, object fieldValue)
    {
        var fieldName = queryConditionAttribute.FieldName.IsNullOrEmpty()
            ? propertyInfo.Name
            : queryConditionAttribute.FieldName;
        var propertyType = propertyInfo.PropertyType;


        var conditionalModels = new List<IConditionalModel>();

        //处理时间范围
        // if (propertyType == typeof(List<DateTime>) && propertyInfo.DeclaringType != null &&
        //     propertyInfo.DeclaringType.FullName == typeof(DateRange).FullName)
        // {
        //     var dateTimeList = (List<DateTime>)fieldValue;
        //     if (dateTimeList.Count != 2)
        //     {
        //         throw new ArgumentException(
        //             "Range condition requires a list with exactly two DateTime values.");
        //     }
        //
        //     conditionalModels.Add(new ConditionalModel()
        //     {
        //         FieldName = UtilMethods.ToUnderLine(fieldName),
        //         ConditionalType = ConditionalType.GreaterThanOrEqual,
        //         FieldValue = dateTimeList[0].ToString("yyyy-MM-dd HH:mm:ss")
        //     });
        //     conditionalModels.Add(new ConditionalModel()
        //     {
        //         FieldName = UtilMethods.ToUnderLine(fieldName),
        //         ConditionalType = ConditionalType.LessThanOrEqual,
        //         FieldValue = dateTimeList[1].ToString("yyyy-MM-dd HH:mm:ss")
        //     });
        //     return conditionalModels;
        // }


        var cSharpTypeName = ""; //指定类型  默认为String
        if (propertyType == typeof(bool?) || propertyType == typeof(bool) || propertyType == typeof(int?) ||
            propertyType == typeof(int) || propertyType == typeof(long?) || propertyType == typeof(long)
            || propertyType == typeof(DateTime?) || propertyType == typeof(DateTime))
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            cSharpTypeName = underlyingType.Name;
        }

        if (propertyType.IsEnum ||
            Nullable.GetUnderlyingType(propertyType)?.IsEnum == true)
        {
            cSharpTypeName = "int";
        }

        //多字段查询
        if (queryConditionAttribute.FieldNameItems.Length > 0)
        {
            var conditionalCollections = new ConditionalCollections
            {
                ConditionalList = []
            };
            foreach (var itemStr in queryConditionAttribute.FieldNameItems)
            {
                conditionalCollections.ConditionalList.Add(
                    new KeyValuePair<WhereType, ConditionalModel>(
                        WhereType.Or,
                        new ConditionalModel
                        {
                            FieldName = UtilMethods.ToUnderLine(itemStr),
                            ConditionalType = ToSugarConditionalType(queryConditionAttribute.ConditionType), FieldValue = fieldValue.ToString()
                        }
                    )
                );
            }

            conditionalModels.Add(conditionalCollections);
            return conditionalModels;
        }

        // 属性一对一
        conditionalModels.Add(new ConditionalModel
        {
            FieldName = UtilMethods.ToUnderLine(fieldName), // 字段名要跟SqlSugarSetup配置的一致
            ConditionalType = ToSugarConditionalType(queryConditionAttribute.ConditionType),
            FieldValue = fieldValue.ToString(),
            CSharpTypeName = cSharpTypeName
        });

        return conditionalModels;
    }

    /// <summary>
    /// 把契约层的查询条件类型映射为 SqlSugar 的条件类型。
    /// 契约层（Ape.Volo.Shared）不引用 ORM，转换集中在这里。
    /// </summary>
    /// <param name="conditionType">契约层条件类型</param>
    /// <returns>SqlSugar 条件类型</returns>
    /// <exception cref="ArgumentOutOfRangeException">条件类型未映射</exception>
    private static ConditionalType ToSugarConditionalType(QueryConditionType conditionType)
    {
        return conditionType switch
        {
            QueryConditionType.Equal => ConditionalType.Equal,
            QueryConditionType.NotEqual => ConditionalType.NoEqual,
            QueryConditionType.Like => ConditionalType.Like,
            QueryConditionType.LikeLeft => ConditionalType.LikeLeft,
            QueryConditionType.LikeRight => ConditionalType.LikeRight,
            QueryConditionType.NotLike => ConditionalType.NoLike,
            QueryConditionType.GreaterThan => ConditionalType.GreaterThan,
            QueryConditionType.GreaterThanOrEqual => ConditionalType.GreaterThanOrEqual,
            QueryConditionType.LessThan => ConditionalType.LessThan,
            QueryConditionType.LessThanOrEqual => ConditionalType.LessThanOrEqual,
            QueryConditionType.In => ConditionalType.In,
            QueryConditionType.NotIn => ConditionalType.NotIn,
            QueryConditionType.IsNull => ConditionalType.IsNullOrEmpty,
            QueryConditionType.IsNotNull => ConditionalType.IsNot,
            _ => throw new ArgumentOutOfRangeException(nameof(conditionType), conditionType, "未映射的查询条件类型")
        };
    }
}
