namespace Ape.Volo.Application.Contracts.Authorization;

/// <summary>
/// 应用服务授权检查，不依赖 HTTP；空角色和权限集合表示仅检查登录身份。
/// </summary>
public interface IApplicationAuthorizationService
{
    /// <summary>
    /// 检查当前身份，角色与权限集合满足任意一项即可；失败抛出授权异常。
    /// </summary>
    /// <param name="roleCodes">允许的角色代码</param>
    /// <param name="permissionCodes">允许的权限代码</param>
    /// <param name="ct">取消令牌</param>
    Task CheckAsync(IReadOnlyCollection<string> roleCodes, IReadOnlyCollection<string> permissionCodes,
        CancellationToken ct = default);
}
