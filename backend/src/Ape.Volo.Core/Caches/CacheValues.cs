using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ape.Volo.Core.Caches;

/// <summary>缓存只保存业务值，读取类型由调用方指定。</summary>
internal static class CacheValues
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        TypeNameHandling = TypeNameHandling.None,
        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
    };

    public static string Serialize(object? value) => value is string text ? text : SerializeJson(value);

    public static string SerializeJson(object? value) => JsonConvert.SerializeObject(value, Settings);

    public static T? DeserializeJson<T>(string value) => JsonConvert.DeserializeObject<T>(value, Settings);

    public static T? Deserialize<T>(string? value) => value == null ? default
        : typeof(T) == typeof(string) ? (T)(object)value
        : DeserializeJson<T>(value);
}
