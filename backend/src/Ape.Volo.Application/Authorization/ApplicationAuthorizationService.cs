using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Shared.Exceptions;

namespace Ape.Volo.Application.Authorization;

/// <summary>
/// 根据当前调用身份和内部权限元数据授权，不依赖 HTTP，也不调用带权限的用户管理入口。
/// </summary>
/// <param name="currentUser">当前身份</param>
/// <param name="permissions">内部角色和权限读取</param>
public sealed class ApplicationAuthorizationService(ICurrentUser currentUser, IPermissionService permissions)
    : IApplicationAuthorizationService
{
    /// <inheritdoc />
    public async Task CheckAsync(IReadOnlyCollection<string> roleCodes, IReadOnlyCollection<string> permissionCodes,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (!currentUser.IsAuthenticated || currentUser.Id <= 0)
            throw new ServiceAuthorizationException(false);
        if (roleCodes.Count == 0 && permissionCodes.Count == 0) return;

        if (roleCodes.Count > 0)
        {
            var currentRoles = await permissions.GetRoleCodesAsync(currentUser.Id, ct);
            if (roleCodes.Any(currentRoles.Contains)) return;
        }
        if (permissionCodes.Count > 0)
        {
            var currentPermissions = await permissions.GetAuthCodeAsync(currentUser.Id, ct);
            if (permissionCodes.Any(currentPermissions.Contains)) return;
        }
        throw new ServiceAuthorizationException(true);
    }
}
