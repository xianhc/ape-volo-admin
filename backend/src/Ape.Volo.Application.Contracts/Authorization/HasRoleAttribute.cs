namespace Ape.Volo.Application.Contracts.Authorization;

/// <summary>
/// 调用应用服务需拥有任意一个角色码；角色码对应 sys_role.AuthCode。
/// </summary>
/// <param name="authCodes">允许访问的角色码</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, Inherited = true)]
public sealed class HasRoleAttribute(params string[] authCodes) : Attribute
{
    /// <summary>
    /// 任意一个匹配即可；同时声明 HasPerm 时，角色与权限满足其一即可。
    /// </summary>
    public string[] AuthCodes { get; } = authCodes;
}
