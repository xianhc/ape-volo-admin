using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Ape.Volo.Common.Reflection;

/// <summary>
/// 从程序集 XML 注释读取模型和字段说明，缺少文档时回退到代码名称
/// </summary>
public static class ModelDescription
{
    private static readonly ConcurrentDictionary<Assembly, Lazy<IReadOnlyDictionary<string, string>>> Documents = new();

    /// <summary>
    /// 取得模型或字段名称，优先使用指定类型的注释，再使用备用类型
    /// </summary>
    /// <param name="type">实体或模型类型</param>
    /// <param name="propertyName">字段名；为空时读取类型说明</param>
    /// <param name="fallbackType">备用 DTO 类型</param>
    /// <returns>注释说明或代码名称</returns>
    public static string GetName(Type type, string? propertyName = null, Type? fallbackType = null)
        => GetSummary(type, propertyName) ?? (fallbackType == null ? null : GetSummary(fallbackType, propertyName))
           ?? propertyName ?? fallbackType?.Name ?? type.Name;

    private static string? GetSummary(Type type, string? propertyName)
    {
        var declaringType = propertyName == null ? type : type.GetProperty(propertyName)?.DeclaringType;
        if (declaringType == null) return null;
        if (declaringType.IsGenericType) declaringType = declaringType.GetGenericTypeDefinition();
        var memberName = propertyName == null
            ? $"T:{declaringType.FullName?.Replace('+', '.')}"
            : $"P:{declaringType.FullName?.Replace('+', '.')}.{propertyName}";
        var document = Documents.GetOrAdd(declaringType.Assembly,
            assembly => new Lazy<IReadOnlyDictionary<string, string>>(() => ReadDocument(assembly))).Value;
        return document.TryGetValue(memberName, out var summary) ? summary : null;
    }

    private static IReadOnlyDictionary<string, string> ReadDocument(Assembly assembly)
    {
        try
        {
            var path = Path.ChangeExtension(assembly.Location, ".xml");
            if (!File.Exists(path)) return new Dictionary<string, string>();
            return XDocument.Load(path).Descendants("member")
                .Where(member => member.Attribute("name") != null && !string.IsNullOrWhiteSpace(member.Element("summary")?.Value))
                .GroupBy(member => member.Attribute("name")!.Value, StringComparer.Ordinal)
                .ToDictionary(group => group.Key,
                    group => Regex.Replace(group.First().Element("summary")!.Value, @"\s+", " ").Trim(),
                    StringComparer.Ordinal);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or XmlException or NotSupportedException)
        {
            return new Dictionary<string, string>();
        }
    }
}
