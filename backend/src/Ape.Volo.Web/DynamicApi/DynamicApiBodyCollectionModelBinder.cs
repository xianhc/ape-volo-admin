using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 正文集合参数绑定：仅支持按参数名包装的对象，如 <c>{ "ids": [1,2] }</c>
/// </summary>
public sealed class DynamicApiBodyCollectionModelBinder : IModelBinder
{
    /// <summary>
    /// 从请求正文读取集合
    /// </summary>
    /// <param name="bindingContext">绑定上下文</param>
    /// <returns>异步任务</returns>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var request = bindingContext.HttpContext.Request;
        request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false,
                   leaveOpen: true))
        {
            body = await reader.ReadToEndAsync(bindingContext.HttpContext.RequestAborted);
        }

        request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            var jsonObject = JObject.Parse(body);
            var fieldName = bindingContext.FieldName ?? bindingContext.ModelName;
            var value = jsonObject.GetValue(fieldName, StringComparison.OrdinalIgnoreCase);

            if (value is not JArray)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                    $"请求正文必须包含数组字段 {fieldName}。");
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            bindingContext.Result = ModelBindingResult.Success(value.ToObject(bindingContext.ModelType));
        }
        catch (JsonException exception)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exception.Message);
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}

/// <summary>
/// 为正文绑定的长整型集合提供 <see cref="DynamicApiBodyCollectionModelBinder"/>
/// </summary>
public sealed class DynamicApiBodyCollectionModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// 仅接管正文来源的长整型集合，包括编排入口使用的 <c>IEnumerable&lt;long&gt;</c>
    /// </summary>
    /// <param name="context">绑定提供者上下文</param>
    /// <returns>模型绑定器，不接管时返回 null</returns>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.BindingInfo?.BindingSource != BindingSource.Body)
        {
            return null;
        }

        var modelType = context.Metadata.ModelType;
        return modelType == typeof(HashSet<long>) || modelType == typeof(List<long>) || modelType == typeof(long[])
               || modelType == typeof(IEnumerable<long>)
            ? new DynamicApiBodyCollectionModelBinder()
            : null;
    }
}
