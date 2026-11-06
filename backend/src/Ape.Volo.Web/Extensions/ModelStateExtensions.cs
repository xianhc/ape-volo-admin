using Ape.Volo.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// MVC 模型状态到 API 错误响应的适配
/// </summary>
public static class ModelStateExtensions
{
    /// <summary>
    /// 合并同一字段的校验提示，绑定异常使用可读的格式错误提示
    /// </summary>
    /// <param name="self">模型校验状态</param>
    /// <returns>按字段归并的错误集合</returns>
    public static ActionError GetErrors(this ModelStateDictionary self)
    {
        var actionError = new ActionError();
        foreach (var item in self)
        {
            if (item.Value.Errors.Count == 0) continue;
            var name = string.IsNullOrWhiteSpace(item.Key) ? "$" : item.Key;
            var messages = item.Value.Errors.Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? name == "$" ? "请求正文格式不正确。" : $"{name}格式不正确。"
                    : error.ErrorMessage)
                .Select(message => message.Trim().TrimEnd('。', '；'))
                .Distinct(StringComparer.Ordinal);
            var message = string.Join("；", messages) + "。";
            actionError.Errors[name] = actionError.Errors.TryGetValue(name, out var existing)
                ? $"{existing.TrimEnd('。')}；{message}" : message;
        }
        return actionError;
    }

    /// <summary>
    /// 将模型校验错误转换为统一的 HTTP 400 响应
    /// </summary>
    /// <param name="self">模型校验状态</param>
    /// <param name="context">当前请求</param>
    /// <returns>错误响应</returns>
    public static ContentResult ToErrorResult(this ModelStateDictionary self, HttpContext context)
    {
        var errors = self.GetErrors();
        return new ContentResult
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentType = "application/json; charset=utf-8",
            Content = new ActionResultVm
            {
                Status = StatusCodes.Status400BadRequest,
                ActionError = errors,
                Message = errors.GetFirstError(),
                Timestamp = DateTime.Now.ToUnixTimeStampMillisecond().ToString(),
                Path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty
            }.ToJson()
        };
    }
}
