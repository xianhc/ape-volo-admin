using System.Reflection;
using Ape.Volo.Common.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ape.Volo.Web.Swagger;

/// <summary>
/// 让 Schema 属性名称与忽略规则跟随 MVC 实际使用的 Newtonsoft.Json 契约。
/// </summary>
/// <param name="options">MVC 序列化设置</param>
public sealed class JsonContractSchemaFilter(IOptions<MvcNewtonsoftJsonOptions> options) : ISchemaFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var resolver = options.Value.SerializerSettings.ContractResolver ?? new CustomContractResolver();
        if (resolver.ResolveContract(context.Type) is not JsonObjectContract contract) return;
        foreach (var property in context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var existing = schema.Properties.Keys.FirstOrDefault(name =>
                string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase));
            if (existing == null) continue;
            var jsonProperty = contract.Properties.FirstOrDefault(value => value.UnderlyingName == property.Name);
            if (jsonProperty?.Ignored == true)
            {
                schema.Properties.Remove(existing);
                schema.Required.Remove(existing);
            }
            else if (jsonProperty?.PropertyName is { } name && name != existing)
            {
                var definition = schema.Properties[existing];
                schema.Properties.Remove(existing);
                schema.Properties[name] = definition;
                if (schema.Required.Remove(existing)) schema.Required.Add(name);
            }
        }
    }
}
