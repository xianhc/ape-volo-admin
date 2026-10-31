using System.Reflection;
using System.Text;
using Ape.Volo.Core.ConfigOptions;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 动态接口端点表：按约定把应用服务接口的方法映射为 HTTP 端点
/// </summary>
public sealed class DynamicApiRegistry
{
    /// <summary>
    /// 动词前缀表：按方法名前缀推导 HTTP 动词，未命中时默认 POST。
    /// 注意：Download/Find 不在 GET 前缀里，按此表它们会落到 POST。
    /// </summary>
    private static readonly (string Method, string[] Prefixes)[] ConventionalPrefixes =
    [
        ("GET", ["GetList", "GetAll", "Get"]),
        ("PUT", ["Put", "Update"]),
        ("DELETE", ["Delete", "Remove"]),
        ("POST", ["Create", "Add", "Insert", "Post"]),
        ("PATCH", ["Patch"])
    ];

    /// <summary>
    /// 生成模块名时要剥掉的后缀
    /// </summary>
    private static readonly string[] ServiceSuffixes = ["Service"];

    private readonly Dictionary<Type, IReadOnlyList<DynamicApiEndpoint>> _endpoints;

    private DynamicApiRegistry(Dictionary<Type, IReadOnlyList<DynamicApiEndpoint>> endpoints,
        IReadOnlyList<string> skipped)
    {
        _endpoints = endpoints;
        Skipped = skipped;
    }

    /// <summary>
    /// 扫描契约程序集，生成端点表
    /// </summary>
    /// <param name="contractsAssembly">契约程序集</param>
    /// <param name="options">动态接口配置</param>
    /// <returns>端点表</returns>
    /// <exception cref="InvalidOperationException">同模块同路由同动词出现重复端点</exception>
    public static DynamicApiRegistry Build(Assembly contractsAssembly, DynamicApiOptions options)
    {
        var excluded = new HashSet<string>(options.ExcludeServices, StringComparer.Ordinal);

        var prefix = options.RoutePrefix.Trim('/');
        var endpoints = new Dictionary<Type, IReadOnlyList<DynamicApiEndpoint>>();
        var skipped = new List<string>();

        var serviceTypes = contractsAssembly.GetExportedTypes()
            .Where(type => type.IsInterface && !type.ContainsGenericParameters)
            .Where(type => type != typeof(IRemoteService) && type != typeof(IApplicationService))
            .Where(type => typeof(IRemoteService).IsAssignableFrom(type))
            .Where(type => type.GetCustomAttribute<RemoteServiceAttribute>()?.IsEnabled != false)
            .Where(type => !excluded.Contains(type.Name))
            .OrderBy(type => type.Name, StringComparer.Ordinal);

        foreach (var serviceType in serviceTypes)
        {
            var name = serviceType.Name;
            if (name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
            {
                name = name[1..];
            }

            var module = ToKebabCase(RemoveServiceSuffix(name));
            var serviceEndpoints = new List<DynamicApiEndpoint>();

            // Type.GetMethods 不包含父接口方法；闭合泛型父契约也在这里展开。
            var methods = serviceType.GetInterfaces().Append(serviceType)
                .SelectMany(type => type.GetMethods()).Distinct();
            foreach (var method in methods)
            {
                if (method.IsSpecialName || method.IsStatic
                    || method.GetCustomAttribute<RemoteServiceAttribute>()?.IsEnabled == false
                    || method.DeclaringType?.GetCustomAttribute<RemoteServiceAttribute>()?.IsEnabled == false)
                {
                    continue;
                }

                // 泛型方法无法生成接口文档，也不适合作为 HTTP 契约
                if (method.IsGenericMethodDefinition)
                {
                    skipped.Add($"{serviceType.Name}.{method.Name}（泛型方法）");
                    continue;
                }

                var httpMethod = ResolveHttpMethod(method.Name);
                var actionName = ToKebabCase(RemoveAsyncSuffix(method.Name));
                serviceEndpoints.Add(new DynamicApiEndpoint(serviceType, method, module, actionName, httpMethod,
                    $"{prefix}/{module}/{actionName}"));
            }

            if (serviceEndpoints.Count > 0)
            {
                endpoints[serviceType] = serviceEndpoints;
            }
        }

        // 路由名规范化后可能相同，必须跨契约检测冲突。
        var conflict = endpoints.Values
            .SelectMany(value => value)
            .GroupBy(endpoint => (endpoint.HttpMethod, endpoint.RouteTemplate))
            .FirstOrDefault(group => group.Count() > 1);
        if (conflict is not null)
        {
            var names = string.Join("、", conflict.Select(endpoint =>
                $"{endpoint.ServiceType.Name}.{endpoint.Method.Name}"));
            throw new InvalidOperationException(
                $"动态接口路由冲突：{conflict.Key.HttpMethod} /{conflict.Key.RouteTemplate} 对应 {names}");
        }

        return new DynamicApiRegistry(endpoints, skipped);
    }

    /// <summary>
    /// 全部端点
    /// </summary>
    public IEnumerable<DynamicApiEndpoint> All => _endpoints.Values.SelectMany(value => value);

    /// <summary>
    /// 参与映射的接口及其端点
    /// </summary>
    public IEnumerable<(Type ServiceType, IReadOnlyList<DynamicApiEndpoint> Endpoints)> Services
        => _endpoints.Select(pair => (pair.Key, pair.Value));

    /// <summary>
    /// 被跳过的接口方法说明
    /// </summary>
    public IReadOnlyList<string> Skipped { get; }

    /// <summary>
    /// 判断接口是否参与映射
    /// </summary>
    /// <param name="serviceType">接口类型</param>
    /// <returns>参与映射返回 true</returns>
    public bool IsExposed(Type serviceType) => _endpoints.ContainsKey(serviceType);

    /// <summary>
    /// 读取接口的端点集合
    /// </summary>
    /// <param name="serviceType">接口类型</param>
    /// <returns>端点集合，未参与映射时为空</returns>
    public IReadOnlyList<DynamicApiEndpoint> GetEndpoints(Type serviceType)
        => _endpoints.TryGetValue(serviceType, out var value) ? value : [];

    /// <summary>
    /// 按方法查找端点
    /// </summary>
    /// <param name="serviceType">接口类型</param>
    /// <param name="method">接口方法</param>
    /// <returns>端点，未参与映射时为 null</returns>
    public DynamicApiEndpoint? Find(Type serviceType, MethodInfo method)
        => GetEndpoints(serviceType).FirstOrDefault(endpoint =>
            endpoint.Method.Module == method.Module && endpoint.Method.MetadataToken == method.MetadataToken);

    private static string RemoveServiceSuffix(string name)
    {
        foreach (var suffix in ServiceSuffixes)
        {
            if (name.EndsWith(suffix, StringComparison.Ordinal))
            {
                return name[..^suffix.Length];
            }
        }

        return name;
    }

    private static string RemoveAsyncSuffix(string name)
        => name.EndsWith("Async", StringComparison.Ordinal) ? name[..^"Async".Length] : name;

    private static string ResolveHttpMethod(string methodName)
    {
        foreach (var (httpMethod, prefixes) in ConventionalPrefixes)
        {
            if (prefixes.Any(prefix => methodName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                return httpMethod;
            }
        }

        return "POST";
    }

    /// <summary>
    /// 驼峰转短横线：CreateUser → create-user，QueryById → query-by-id
    /// </summary>
    /// <param name="name">驼峰名称</param>
    /// <returns>短横线名称</returns>
    private static string ToKebabCase(string name)
    {
        var builder = new StringBuilder(name.Length + 8);
        for (var index = 0; index < name.Length; index++)
        {
            var current = name[index];
            if (!char.IsUpper(current))
            {
                builder.Append(current);
                continue;
            }

            var hasPrevious = index > 0;
            var previousIsLower = hasPrevious && char.IsLower(name[index - 1]);
            var nextIsLower = index + 1 < name.Length && char.IsLower(name[index + 1]);
            if (hasPrevious && (previousIsLower || nextIsLower))
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(current));
        }

        return builder.ToString();
    }
}
