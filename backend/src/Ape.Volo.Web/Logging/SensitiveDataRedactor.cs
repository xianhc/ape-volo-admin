using Ape.Volo.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ape.Volo.Web.Logging;

/// <summary>
/// 在审计和异常日志写入前移除敏感字段值。
/// </summary>
public static class SensitiveDataRedactor
{
    private const string RedactedValue = "***REDACTED***";

    /// <summary>
    /// 序列化对象并脱敏其中的敏感字段
    /// </summary>
    /// <param name="value">待序列化的对象</param>
    /// <returns>脱敏后的 JSON 字符串</returns>
    public static string Serialize(object value)
    {
        return RedactJson(value.ToJson());
    }

    /// <summary>
    /// 对 JSON 字符串中的敏感字段值做脱敏，非法 JSON 原样返回
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <returns>脱敏后的 JSON 字符串</returns>
    public static string RedactJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        try
        {
            var token = JToken.Parse(json);
            RedactToken(token);
            return token.ToString(Formatting.None);
        }
        catch (JsonReaderException)
        {
            return json;
        }
    }

    private static void RedactToken(JToken token)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                if (IsSensitiveName(property.Name))
                {
                    property.Value = RedactedValue;
                }
                else
                {
                    RedactToken(property.Value);
                }
            }

            return;
        }

        if (token is JArray array)
        {
            foreach (var item in array)
            {
                RedactToken(item);
            }
        }
    }

    private static bool IsSensitiveName(string name)
    {
        return name.Equals("pass", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
               name.Equals("authorization", StringComparison.OrdinalIgnoreCase);
    }
}
