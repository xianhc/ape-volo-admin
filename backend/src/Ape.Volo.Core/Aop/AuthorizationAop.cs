using System.Reflection;
using Ape.Volo.Application.Contracts.Authorization;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Authorization;

namespace Ape.Volo.Core.Aop;

/// <summary>
/// 在事务、缓存和业务方法之前检查应用服务授权；没有声明授权的内部方法按原逻辑执行。
/// </summary>
/// <param name="authorization">延迟解析检查器，避免其内部权限读取再次构造授权依赖</param>
public sealed class AuthorizationAop(Lazy<IApplicationAuthorizationService> authorization) : IInterceptor, IAsyncInterceptor
{
    /// <inheritdoc />
    public void Intercept(IInvocation invocation) => new AsyncDeterminationInterceptor(this).Intercept(invocation);

    /// <inheritdoc />
    public void InterceptSynchronous(IInvocation invocation)
    {
        if (GetRules(invocation).Count != 0)
            throw new InvalidOperationException($"{invocation.Method.Name} 声明了授权，请使用 Task 或 Task<T> 返回类型。");
        invocation.Proceed();
    }

    /// <inheritdoc />
    public void InterceptAsynchronous(IInvocation invocation)
    {
        var rules = GetRules(invocation);
        if (rules.Count == 0)
        {
            invocation.Proceed();
            return;
        }
        invocation.ReturnValue = InvokeAsync(invocation, invocation.CaptureProceedInfo(), rules);
    }

    /// <inheritdoc />
    public void InterceptAsynchronous<TResult>(IInvocation invocation)
    {
        var rules = GetRules(invocation);
        if (rules.Count == 0)
        {
            invocation.Proceed();
            return;
        }
        invocation.ReturnValue = InvokeAsync<TResult>(invocation, invocation.CaptureProceedInfo(), rules);
    }

    private async Task InvokeAsync(IInvocation invocation, IInvocationProceedInfo proceed, IReadOnlyList<Rule> rules)
    {
        await CheckAsync(invocation, rules);
        proceed.Invoke();
        await (Task)invocation.ReturnValue;
    }

    private async Task<TResult> InvokeAsync<TResult>(IInvocation invocation, IInvocationProceedInfo proceed,
        IReadOnlyList<Rule> rules)
    {
        await CheckAsync(invocation, rules);
        proceed.Invoke();
        return await (Task<TResult>)invocation.ReturnValue;
    }

    private async Task CheckAsync(IInvocation invocation, IReadOnlyList<Rule> rules)
    {
        var ct = invocation.Arguments.OfType<CancellationToken>().FirstOrDefault();
        ct.ThrowIfCancellationRequested();
        foreach (var rule in rules)
        {
            await authorization.Value.CheckAsync(rule.Roles, rule.Permissions, ct);
        }
        ct.ThrowIfCancellationRequested();
    }

    private static IReadOnlyList<Rule> GetRules(IInvocation invocation)
    {
        // 实现方法和类为主要声明位置，同时支持契约声明；不能读取当前 HTTP 端点的特性代替服务声明。
        MemberInfo[] sources = [invocation.TargetType, invocation.MethodInvocationTarget ?? invocation.Method,
            invocation.Method.DeclaringType!, invocation.Method];
        var groups = sources.Distinct().Select(source => source.GetCustomAttributes(inherit: true).ToArray()).ToArray();
        if (groups.Any(attributes => attributes.OfType<IAllowAnonymous>().Any())) return [];

        var rules = new List<Rule>();
        foreach (var attributes in groups)
        {
            var roles = attributes.OfType<HasRoleAttribute>().SelectMany(attribute => attribute.AuthCodes).ToArray();
            var permissions = attributes.OfType<HasPermAttribute>().SelectMany(attribute => attribute.AuthCodes).ToArray();
            if (attributes.Any(attribute => attribute is HasRoleAttribute or HasPermAttribute))
            {
                if (roles.Length + permissions.Length == 0 || roles.Concat(permissions).Any(string.IsNullOrWhiteSpace))
                    throw new InvalidOperationException("HasRole/HasPerm 必须指定非空的授权代码。");
                rules.Add(new Rule(roles, permissions));
            }

            foreach (var attribute in attributes.OfType<IAuthorizeData>())
            {
                // 应用服务不绑定 HTTP 认证方案；Policy 对应一个权限点，Roles 对应角色代码。
                if (!string.IsNullOrWhiteSpace(attribute.AuthenticationSchemes))
                    throw new InvalidOperationException("应用服务授权不支持 HTTP AuthenticationSchemes，请在宿主配置认证。");
                var requiredRoles = attribute.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
                if (requiredRoles.Length > 0) rules.Add(new Rule(requiredRoles, []));
                if (!string.IsNullOrWhiteSpace(attribute.Policy)) rules.Add(new Rule([], [attribute.Policy]));
                if (requiredRoles.Length == 0 && string.IsNullOrWhiteSpace(attribute.Policy)) rules.Add(new Rule([], []));
            }
        }
        return rules;
    }

    private sealed record Rule(string[] Roles, string[] Permissions);
}
