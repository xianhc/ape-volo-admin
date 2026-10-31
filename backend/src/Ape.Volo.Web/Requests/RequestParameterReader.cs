using System.Text;
using Ape.Volo.Shared.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ape.Volo.Web.Requests;

/// <summary>
/// 异步读取入站请求参数，保留请求流位置；脱敏由日志调用方负责。
/// </summary>
public static class RequestParameterReader
{
    /// <summary>
    /// 合并 Query、Form 和 JSON 参数，JSON 同名值最后覆盖。
    /// </summary>
    /// <param name="request">入站 HTTP 请求</param>
    /// <param name="ct">取消令牌</param>
    public static async Task<Dictionary<string, object>> ReadAsync(HttpRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var values = request.Query.ToDictionary(item => item.Key, item => (object)item.Value.ToString(), StringComparer.Ordinal);
        if (request.HasFormContentType)
        {
            foreach (var item in await request.ReadFormAsync(ct)) values.TryAdd(item.Key, item.Value.ToString());
        }
        else if (request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true ||
                 request.ContentType?.Contains("+json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                var text = await ReadBodyAsync(request, ct);
                if (!string.IsNullOrWhiteSpace(text) && JToken.Parse(text) is JObject json)
                    foreach (var property in json.Properties()) values[property.Name] = property.Value;
            }
            catch (JsonException) { throw new BadRequestException("请求 JSON 格式错误。"); }
        }
        return values;
    }

    /// <summary>
    /// 读取原始正文用于签名验证，保持流开启并还原原位置。
    /// </summary>
    /// <param name="request">入站 HTTP 请求</param>
    /// <param name="ct">取消令牌</param>
    public static async Task<string> ReadBodyAsync(HttpRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        request.EnableBuffering();
        var position = request.Body.Position;
        try
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            return await reader.ReadToEndAsync(ct);
        }
        finally { request.Body.Position = position; }
    }
}
