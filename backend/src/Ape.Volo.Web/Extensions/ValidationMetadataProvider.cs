using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Ape.Volo.Application.Mapping;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// 为 MVC 校验提供实体字段说明和中文默认提示
/// </summary>
public sealed class ValidationMetadataProvider : IDisplayMetadataProvider, IValidationMetadataProvider
{
    private readonly IReadOnlyDictionary<Type, Type> _entities = typeof(AutoMappingRegister).Assembly
        .GetCustomAttributes<AutoMappingAttribute>().Where(mapping => mapping.ToEntity)
        .ToDictionary(mapping => mapping.TargetType, mapping => mapping.SourceType);

    /// <inheritdoc />
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        var modelType = context.Key.ContainerType ?? context.Key.ModelType;
        var type = _entities.TryGetValue(modelType, out var entityType) ? entityType : modelType;
        var name = ModelDescription.GetName(type, context.Key.ContainerType == null ? null : context.Key.Name, modelType);
        context.DisplayMetadata.DisplayName = () => name;
    }

    /// <inheritdoc />
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if (attribute.ErrorMessage != null || attribute.ErrorMessageResourceType != null
                                               || attribute.ErrorMessageResourceName != null) continue;
            attribute.ErrorMessage = attribute switch
            {
                RequiredAttribute => "{0}不能为空。",
                StringLengthAttribute => "{0}长度应在{2}到{1}个字符之间。",
                MinLengthAttribute => "{0}长度不能小于{1}。",
                MaxLengthAttribute => "{0}长度不能大于{1}。",
                RangeAttribute => "{0}必须在{1}到{2}之间。",
                RegularExpressionAttribute or EmailAddressAttribute or PhoneAttribute or UrlAttribute => "{0}格式不正确。",
                CompareAttribute => "{0}与{1}不一致。",
                _ => null
            };
        }
    }

    /// <summary>
    /// 为动态接口和显式控制器统一配置校验元数据与绑定提示
    /// </summary>
    /// <param name="options">MVC 配置</param>
    public static void Configure(MvcOptions options)
    {
        options.ModelMetadataDetailsProviders.Add(new ValidationMetadataProvider());
        var messages = options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((_, name) => $"{name}格式不正确。");
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => "请求参数格式不正确。");
        messages.SetUnknownValueIsInvalidAccessor(name => $"{name}格式不正确。");
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(() => "请求参数格式不正确。");
        messages.SetValueIsInvalidAccessor(_ => "请求参数格式不正确。");
        messages.SetValueMustNotBeNullAccessor(_ => "必填参数不能为空。");
        messages.SetValueMustBeANumberAccessor(name => $"{name}必须是数字。");
        messages.SetNonPropertyValueMustBeANumberAccessor(() => "请求参数必须是数字。");
        messages.SetMissingBindRequiredValueAccessor(name => $"{name}不能为空。");
        messages.SetMissingKeyOrValueAccessor(() => "请求参数缺少键或值。");
        messages.SetMissingRequestBodyRequiredValueAccessor(() => "请求正文不能为空。");
    }
}
