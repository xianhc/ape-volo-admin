using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ape.Volo.Core.Utils;

/// <summary>
/// 数据错误
/// </summary>
public static class ValidationError
{
    /// <summary>
    /// 数据已存在
    /// </summary>
    /// <param name="instance">类型</param>
    /// <param name="propertyName">值</param>
    /// <returns></returns>
    public static string IsExist<T>(T instance, string propertyName)
    {
        // 获取属性元数据
        var property = typeof(T).GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (property == null)
        {
            throw new ArgumentException($"属性 '{propertyName}' 在类型 '{typeof(T).Name}' 中不存在。");
        }

        // 获取属性值
        var value = property.GetValue(instance);

        // 获取 Display 特性
        var display = property.GetCustomAttribute<DisplayAttribute>();
        var displayName = display == null ? property.Name : display.Name;

        // 返回本地化字符串
        return $"{displayName}=>{value}=>已存在!";
    }

    /// <summary>
    /// 数据不存在
    /// </summary>
    /// <returns></returns>
    public static string NotExist()
    {
        // 返回本地化字符串
        return "数据不存在";
    }

    /// <summary>
    /// 数据不存在
    /// </summary>
    /// <param name="instance">类型</param>
    /// <param name="displayName">占位符</param>
    /// <param name="propertyName">值</param>
    /// <returns></returns>
    public static string NotExist<T>(T instance, string displayName, string propertyName)
    {
        // 获取属性元数据
        var property = typeof(T).GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (property == null)
        {
            throw new ArgumentException($"属性 '{propertyName}' 在类型 '{typeof(T).Name}' 中不存在。");
        }

        // 获取属性值
        var value = property.GetValue(instance);

        // 获取 Display 特性
        // var display = property.GetCustomAttribute<DisplayAttribute>();
        // var displayName = display == null ? property.Name : display.Name;

        // 返回本地化字符串
        return $"{displayName}=>{value}=>不存在!";
    }

    /// <summary>
    /// 数据存在关联
    /// </summary>
    /// <returns></returns>
    public static string DataAssociationExists()
    {
        // 返回本地化字符串
        return "存在数据关联，请解除后再试！";
    }

    /// <summary>
    /// 必填项
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="propertyName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string Required<T>(T instance, string propertyName)
    {
        // 获取属性元数据
        var property = typeof(T).GetProperty(propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (property == null)
        {
            throw new ArgumentException($"属性 '{propertyName}' 在类型 '{typeof(T).Name}' 中不存在。");
        }

        // 获取 Display 特性
        var display = property.GetCustomAttribute<DisplayAttribute>();
        var displayName = display == null ? property.Name : display.Name;

        // 返回本地化字符串
        return $"{displayName} 是必填项";
    }
}
