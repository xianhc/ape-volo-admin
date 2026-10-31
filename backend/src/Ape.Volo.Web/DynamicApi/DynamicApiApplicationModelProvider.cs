using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ape.Volo.Web.DynamicApi;

/// <summary>
/// 动态接口应用模型提供者：直接把契约层接口构造成控制器模型。
/// 不走 <c>DefaultApplicationModelProvider</c>，因为它会沿 <c>BaseType</c> 查找路由特性，而接口的 <c>BaseType</c> 为 null。
/// </summary>
/// <param name="registry">端点表</param>
public sealed class DynamicApiApplicationModelProvider(DynamicApiRegistry registry) : IApplicationModelProvider
{
    /// <summary>
    /// 排在默认提供者之前，先把自己的控制器模型放进结果集
    /// </summary>
    public int Order => -2000;

    /// <summary>
    /// 构造控制器模型
    /// </summary>
    /// <param name="context">提供者上下文</param>
    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (var (serviceType, endpoints) in registry.Services)
        {
            var controller = new ControllerModel(serviceType.GetTypeInfo(),
                serviceType.GetCustomAttributes(inherit: true).ToArray())
            {
                ControllerName = endpoints[0].Module
            };

            // HTTP 只检查认证；权限由 DI 应用服务代理执行，进程内调用也受约束。
            controller.Filters.Add(new AuthorizeFilter());
            // 统一恢复原控制器的 ModelState 校验，非法输入不得进入应用服务。
            controller.Filters.Add(new DynamicApiValidationFilter());
            // 返回 List<ExportBase> 时转成 Excel 文件
            controller.Filters.Add(new DynamicApiExportFilter());
            // 业务失败（OperateResult.IsSuccess = false）还原成 400
            controller.Filters.Add(new DynamicApiOperateResultFilter());
            // 动态端点同样出现在接口文档里
            controller.ApiExplorer.IsVisible = true;

            foreach (var endpoint in endpoints)
            {
                var action = CreateAction(endpoint);
                // 手工构造的模型不会自动回填所属控制器，路由展平阶段会读它
                action.Controller = controller;
                controller.Actions.Add(action);
            }

            context.Result.Controllers.Add(controller);
        }
    }

    /// <summary>
    /// 默认提供者执行完毕后无需处理
    /// </summary>
    /// <param name="context">提供者上下文</param>
    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
    }

    /// <summary>
    /// 构造单个动作模型：路由、动词与参数绑定
    /// </summary>
    /// <param name="endpoint">端点</param>
    /// <returns>动作模型</returns>
    private static ActionModel CreateAction(DynamicApiEndpoint endpoint)
    {
        var method = endpoint.Method;
        var action = new ActionModel(method, method.GetCustomAttributes(inherit: true).ToArray())
        {
            ActionName = endpoint.ActionName
        };

        var selector = new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(endpoint.RouteTemplate))
        };
        selector.ActionConstraints.Add(new HttpMethodActionConstraint([endpoint.HttpMethod]));
        // 手工创建的 Selector 不会自动复制接口特性，权限处理器从端点元数据读取它们。
        foreach (var attribute in action.Attributes)
        {
            selector.EndpointMetadata.Add(attribute);
        }
        action.Selectors.Add(selector);

        foreach (var parameter in method.GetParameters())
        {
            // ParameterModel 不会从 ParameterInfo 带出名字，必须显式赋值，否则模型绑定拿不到参数名
            var parameterModel = new ParameterModel(parameter,
                parameter.GetCustomAttributes(inherit: true).ToArray())
            {
                ParameterName = parameter.Name ?? string.Empty
            };

            // 取消令牌由框架注入；GET 类动词不带正文，简单类型走路由与查询串
            if (parameter.ParameterType != typeof(CancellationToken)
                && AllowsBody(endpoint.HttpMethod)
                && (IsLongCollection(parameter.ParameterType) || !IsSimpleType(parameter.ParameterType)))
            {
                parameterModel.BindingInfo = BindingInfo.GetBindingInfo([new FromBodyAttribute()]);
            }

            action.Parameters.Add(parameterModel);
        }

        return action;
    }

    /// <summary>
    /// 判断动词是否允许正文。DELETE 允许，因为批量删除的主键集合在正文里
    /// </summary>
    /// <param name="httpMethod">HTTP 动词</param>
    /// <returns>允许正文返回 true</returns>
    private static bool AllowsBody(string httpMethod) => httpMethod is not ("GET" or "HEAD" or "TRACE");

    /// <summary>
    /// 判断是否为长整型集合，这类参数统一走正文
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns>长整型集合返回 true</returns>
    private static bool IsLongCollection(Type type)
        => type == typeof(HashSet<long>) || type == typeof(List<long>) || type == typeof(long[])
           || type == typeof(IEnumerable<long>);

    /// <summary>
    /// 判断是否为基础类型
    /// </summary>
    /// <param name="type">类型</param>
    /// <returns>基础类型返回 true</returns>
    private static bool IsSimpleType(Type type)
    {
        var target = Nullable.GetUnderlyingType(type) ?? type;
        return target.IsPrimitive
               || target.IsEnum
               || target == typeof(string)
               || target == typeof(decimal)
               || target == typeof(DateTime)
               || target == typeof(DateOnly)
               || target == typeof(TimeOnly)
               || target == typeof(TimeSpan)
               || target == typeof(Guid);
    }
}
