namespace Ape.Volo.Application.Contracts.Authorization;

/// <summary>
/// 调用应用服务需拥有任意一个权限码，由应用服务授权拦截器执行。
/// </summary>
/// <param name="authCodes">允许访问的权限码</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, Inherited = true)]
public sealed class HasPermAttribute(params string[] authCodes) : Attribute
{
    /// <summary>
    /// 任意一个匹配即可；同时声明 HasRole 时，角色与权限满足其一即可。
    /// </summary>
    public string[] AuthCodes { get; } = authCodes;
}
