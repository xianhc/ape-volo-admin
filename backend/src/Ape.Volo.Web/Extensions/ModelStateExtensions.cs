using Ape.Volo.Common.IdGenerator;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// MVC 模型状态到 API 错误响应的适配。
/// </summary>
public static class ModelStateExtensions
{
    /// <summary>
    /// 获取模型验证错误信息
    /// 常用的DataAnnotations注解 
    /// 1）非空验证  [Required]
    /// 2）长度验证 [StringLength(100, MinimumLength = 10)]
    /// 3）正则表达式验证 [RegularExpression("your expression")]
    /// 4）值范围验证 [Range(10, 100)]
    /// 5）对比验证 [Compare("Name")]
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static ActionError GetErrors(this ModelStateDictionary self)
    {
        var actionError = new ActionError
        {
            Errors = new Dictionary<string, string>()
        };
        foreach (var item in self)
        {
            if (item.Value.ValidationState != ModelValidationState.Invalid) continue;
            var name = string.IsNullOrEmpty(item.Key) ? IdHelper.NextId().ToString() : item.Key;
            actionError.Errors.Add(name, item.Value.Errors.FirstOrDefault()?.ErrorMessage ?? string.Empty);
        }

        return actionError;
    }
}
