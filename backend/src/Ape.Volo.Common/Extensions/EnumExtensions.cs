using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ape.Volo.Common.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// 获取枚举显示名称
    /// </summary>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static string GetDisplayName(this Enum enumValue)
    {
        // 获取枚举类型
        var type = enumValue.GetType();
        // 获取枚举成员的成员信息
        var memberInfo = type.GetMember(enumValue.ToString());

        if (memberInfo.Length > 0)
        {
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length > 0)
            {
                return ((DisplayAttribute)attributes[0]).Name ?? enumValue.ToString();
            }
        }

        return enumValue.ToString();
    }


    public static string GetDisplayName(Type type, string propertyName)
    {
        // 获取属性信息
        var property = type.GetProperty(propertyName);
        if (property == null)
        {
            throw new ArgumentException($"Property '{propertyName}' not found on type '{type.FullName}'");
        }

        // 获取Display特性
        var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? "No Display Attribute Found";
    }
}
