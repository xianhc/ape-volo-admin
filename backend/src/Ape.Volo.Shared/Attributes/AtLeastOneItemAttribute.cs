using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Ape.Volo.Shared.Attributes;

/// <summary>
/// 集合必须至少包含一个项目。
/// </summary>
public class AtLeastOneItemAttribute : ValidationAttribute
{
    /// <summary>
    /// 校验集合至少包含一个元素。
    /// </summary>
    /// <param name="value">被校验的值</param>
    /// <param name="validationContext">校验上下文</param>
    /// <returns>校验结果</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 获取当前属性的元数据
        var property = validationContext.ObjectType.GetProperty(validationContext.MemberName ?? string.Empty);
        var displayAttribute = property?.GetCustomAttribute<DisplayAttribute>();
        var displayName = displayAttribute?.Name ?? validationContext.MemberName;

        // 验证逻辑：检查集合是否为空
        if (value is IEnumerable list && list.Cast<object>().Any())
        {
            return ValidationResult.Success;
        }

        return new ValidationResult($"{displayName} 集合必须至少包含一个项目");
    }
}
