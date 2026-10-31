using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Ape.Volo.Common.Converters;
using Newtonsoft.Json;

namespace Ape.Volo.Common.Extensions;

public static class ObjectExtensions
{
    private static BindingFlags BindingFlags =>
        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;


    /// <summary>
    /// 判断是否为Null或者空
    /// </summary>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static bool IsNullOrEmpty([NotNullWhen(false)] this object? obj)
    {
        if (obj == null)
            return true;
        string? objStr = obj.ToString();
        return string.IsNullOrEmpty(objStr);
    }

    /// <summary>
    /// 不等于NULL？
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsNotNull([NotNullWhen(true)] this object? obj)
    {
        return obj != null;
    }

    /// <summary>
    /// 等于NULL？
    /// </summary>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static bool IsNull([NotNullWhen(false)] this object? obj)
    {
        return obj == null;
    }

    /// <summary>
    /// 将对象序列化成Json字符串
    /// </summary>
    /// <param name="obj">需要序列化的对象</param>
    /// <returns></returns>
    public static string ToJson(this object? obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.None, CreateJsonSettings(NullValueHandling.Include));
    }

    /// <summary>
    /// 将对象序列化成Json字符串,同时忽略null字段
    /// </summary>
    /// <param name="obj">需要序列化的对象</param>
    /// <returns></returns>
    public static string ToJsonByIgnore(this object? obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.None, CreateJsonSettings(NullValueHandling.Ignore));
    }

    private static JsonSerializerSettings CreateJsonSettings(NullValueHandling nullValueHandling)
    {
        return new JsonSerializerSettings
        {
            // 设置为驼峰命名
            ContractResolver = new CustomContractResolver(),
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = nullValueHandling
        };
    }

    /// <summary>
    /// 实体类转json数据，速度快
    /// </summary>
    /// <param name="t">实体类</param>
    /// <returns></returns>
    public static string? EntityToJson(this object? t)
    {
        if (t == null)
            return null;
        string jsonStr = "";
        jsonStr += "{";
        PropertyInfo[] infos = t.GetType().GetProperties();
        for (int i = 0; i < infos.Length; i++)
        {
            jsonStr = jsonStr + "\"" + infos[i].Name + "\":\"" + infos[i].GetValue(t) + "\"";
            if (i != infos.Length - 1)
                jsonStr += ",";
        }

        jsonStr += "}";
        return jsonStr;
    }

    /// <summary>
    /// 深复制
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static T? DeepClone<T>(this T? obj) where T : class
    {
        if (obj == null)
            return null;

        return obj.ToJson().ToObject<T>();
    }

    /// <summary>
    /// 将对象序列化为XML字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static string? ToXmlStr<T>(this T obj)
    {
        return ToXmlStr(obj, null);
    }

    /// <summary>
    /// 将对象序列化为XML字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">对象</param>
    /// <param name="rootNodeName">根节点名(建议设为xml)</param>
    /// <returns></returns>
    public static string? ToXmlStr<T>(this T obj, string? rootNodeName)
    {
        return JsonConvert.DeserializeXmlNode(obj.ToJson(), rootNodeName)?.InnerXml;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static int ToInt(this object? thisValue)
    {
        return ToInt(thisValue, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static int ToInt(this object? thisValue, int errorValue)
    {
        return ParseOrDefault(thisValue, errorValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static double ToMoney(this object? thisValue)
    {
        return ToMoney(thisValue, 0d);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static double ToMoney(this object? thisValue, double errorValue)
    {
        return ParseOrDefault(thisValue, errorValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static string ToString(this object? thisValue)
    {
        return ToString(thisValue, string.Empty);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static string ToString(this object? thisValue, string errorValue)
    {
        if (thisValue != null) return thisValue.ToString()?.Trim() ?? string.Empty;
        return errorValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static decimal ToDecimal(this object? thisValue)
    {
        return ToDecimal(thisValue, 0m);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static decimal ToDecimal(this object? thisValue, decimal errorValue)
    {
        return ParseOrDefault(thisValue, errorValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static DateTime ToDate(this object? thisValue)
    {
        return ToDate(thisValue, DateTime.MinValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <param name="errorValue"></param>
    /// <returns></returns>
    public static DateTime ToDate(this object? thisValue, DateTime errorValue)
    {
        return ParseOrDefault(thisValue, errorValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="thisValue"></param>
    /// <returns></returns>
    public static bool ToBool(this object? thisValue)
    {
        return ParseOrDefault(thisValue, false);
    }

    private static T ParseOrDefault<T>(object? value, T errorValue) where T : IParsable<T>
    {
        return value != null && value != DBNull.Value && T.TryParse(value.ToString(), null, out var parsed)
            ? parsed
            : errorValue;
    }

    /// <summary>
    /// 是否拥有某属性
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="propertyName">属性名</param>
    /// <returns></returns>
    public static bool ContainsProperty(this object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName, BindingFlags) != null;
    }

    /// <summary>
    /// 获取某属性值
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="propertyName">属性名</param>
    /// <returns></returns>
    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        var pi = obj.GetType().GetProperty(propertyName, BindingFlags);
        if (pi == null) return null;
        var value = pi.GetValue(obj);
        if (value == null) return null;
        var type = pi.PropertyType;
        if (type == typeof(DateTime))
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss");
        }

        if (type.IsEnum)
        {
            return (int)value;
        }

        return value;
    }

    /// <summary>
    /// 设置某属性值
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="propertyName">属性名</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static void SetPropertyValue(this object obj, string propertyName, object? value)
    {
        obj.GetType().GetProperty(propertyName, BindingFlags)?.SetValue(obj, value);
    }

    /// <summary>
    /// 是否拥有某字段
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="fieldName">字段名</param>
    /// <returns></returns>
    public static bool ContainsField(this object obj, string fieldName)
    {
        return obj.GetType().GetField(fieldName, BindingFlags) != null;
    }

    /// <summary>
    /// 获取某字段值
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="fieldName">字段名</param>
    /// <returns></returns>
    public static object? GetGetFieldValue(this object obj, string fieldName)
    {
        return obj.GetType().GetField(fieldName, BindingFlags)?.GetValue(obj);
    }

    /// <summary>
    /// 设置某字段值
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="fieldName">字段名</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    public static void SetFieldValue(this object obj, string fieldName, object? value)
    {
        obj.GetType().GetField(fieldName, BindingFlags)?.SetValue(obj, value);
    }

    /// <summary>
    /// 改变实体类型
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="targetType">目标类型</param>
    /// <returns></returns>
    public static object? ChangeType(this object? obj, Type targetType)
    {
        return obj.ToJson().ToObject(targetType);
    }

    /// <summary>
    /// 改变实体类型
    /// </summary>
    /// <typeparam name="T">目标泛型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static T? ChangeType<T>(this object? obj)
    {
        return obj.ToJson().ToObject<T>();
    }

    /// <summary>
    /// 改变类型
    /// </summary>
    /// <param name="obj">原对象</param>
    /// <param name="targetType">目标类型</param>
    /// <returns></returns>
    public static object? ChangeType_ByConvert(this object? obj, Type targetType)
    {
        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
        {
            NullableConverter newNullableConverter = new NullableConverter(targetType);
            return obj == null ? null : newNullableConverter.ConvertFrom(obj);
        }

        return Convert.ChangeType(obj, targetType);
    }

    public static string GetGenericTypeName(this Type type)
    {
        string typeName;

        if (type.IsGenericType)
        {
            var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
            typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
        }
        else
        {
            typeName = type.Name;
        }

        return typeName;
    }

    public static string GetGenericTypeName(this object obj)
    {
        return obj.GetType().GetGenericTypeName();
    }
}
