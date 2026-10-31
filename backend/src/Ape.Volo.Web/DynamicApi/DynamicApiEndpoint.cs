using System.Reflection;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 动态接口端点：一个应用服务方法对应的 HTTP 契约
/// </summary>
public sealed class DynamicApiEndpoint
{
    /// <summary>
    /// 初始化 <see cref="DynamicApiEndpoint"/> 的新实例。
    /// </summary>
    /// <param name="serviceType">应用服务接口</param>
    /// <param name="method">接口方法</param>
    /// <param name="module">模块名</param>
    /// <param name="actionName">动作名</param>
    /// <param name="httpMethod">HTTP 动词</param>
    /// <param name="routeTemplate">路由模板</param>
    public DynamicApiEndpoint(Type serviceType, MethodInfo method, string module, string actionName,
        string httpMethod, string routeTemplate)
    {
        ServiceType = serviceType;
        Method = method;
        Module = module;
        ActionName = actionName;
        HttpMethod = httpMethod;
        RouteTemplate = routeTemplate;
    }

    /// <summary>
    /// 应用服务接口
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// 接口方法
    /// </summary>
    public MethodInfo Method { get; }

    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// 动作名
    /// </summary>
    public string ActionName { get; }

    /// <summary>
    /// HTTP 动词
    /// </summary>
    public string HttpMethod { get; }

    /// <summary>
    /// 路由模板
    /// </summary>
    public string RouteTemplate { get; }

    /// <summary>
    /// 用于启动日志与冲突检测的显示文本
    /// </summary>
    /// <returns>形如 <c>GET /api/position/get-list</c></returns>
    public override string ToString() => $"{HttpMethod} /{RouteTemplate}";
}
