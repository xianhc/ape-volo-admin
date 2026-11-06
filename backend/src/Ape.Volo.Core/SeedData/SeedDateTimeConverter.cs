using Newtonsoft.Json;

namespace Ape.Volo.Core.SeedData;

/// <summary>
/// 种子数据日期转换器，空日期默认取初始化时的当前时间。
/// </summary>
public sealed class SeedDateTimeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateTime);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return DateTime.Now; // 设置为当前系统时间
        }

        if (reader.TokenType == JsonToken.Date)
        {
            return reader.Value as DateTime? ?? DateTime.Now; // 直接返回日期时间
        }

        if (reader.TokenType == JsonToken.String)
        {
            if (reader.Value != null && DateTime.TryParse(reader.Value.ToString(), out DateTime date))
            {
                return date;
            }

            throw new JsonSerializationException($"Invalid date format: {reader.Value}");
        }

        throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value != null) writer.WriteValue(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
    }
}
