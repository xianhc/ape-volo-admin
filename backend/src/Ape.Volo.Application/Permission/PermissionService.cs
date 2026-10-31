using Ape.Volo.Application.Contracts.Permission.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Permission;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Entity.Permission.Users;

namespace Ape.Volo.Application.Permission;

/// <summary>
/// 权限服务
/// </summary>
public class PermissionService : IPermissionService
{
    #region 字段

    private readonly ISugarQueryProvider _sugarQueries;
    private readonly IPermissionCacheService _permissionCache;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 <see cref="PermissionService"/> 的新实例。
    /// </summary>
    /// <param name="sugarQueries">不受数据权限限制的查询入口</param>
    /// <param name="permissionCache">权限缓存入口</param>
    public PermissionService(ISugarQueryProvider sugarQueries, IPermissionCacheService permissionCache)
    {
        _sugarQueries = sugarQueries;
        _permissionCache = permissionCache;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    public Task<List<string>> GetRoleCodesAsync(long userId, CancellationToken ct = default)
        => _permissionCache.GetOrCreateAsync(PermissionCacheItem.RoleCodes, userId, () => _sugarQueries.QueryWithoutDataScopeAsync(async db =>
        {
            var roles = await db.Queryable<User, UserRole, Role>((user, link, role) =>
                    user.Id == link.UserId && link.RoleId == role.Id)
                .Where((user, link, role) => user.Id == userId && user.Enabled)
                .Select((user, link, role) => role.AuthCode).Distinct().ToListAsync(ct);
            return roles.Where(code => !string.IsNullOrWhiteSpace(code)).ToList();
        }, ct), ct);

    /// <inheritdoc />
    public Task<List<string>> GetAuthCodeAsync(long userId, CancellationToken ct = default)
    {
        // 授权码属于权限元数据，不受当前账号的创建范围限制。
        return _permissionCache.GetOrCreateAsync(PermissionCacheItem.AuthCodes, userId, () => _sugarQueries.QueryWithoutDataScopeAsync(async db =>
        {
            var authCodeList = await db
                .Queryable<UserRole, RoleMenu, Menu>((ur, rm, m) => ur.RoleId == rm.RoleId && rm.MenuId == m.Id)
                .GroupBy((ur, rm, m) => m.AuthCode)
                .Where((ur, rm, m) => ur.UserId == userId && m.Enabled && m.MenuType != MenuType.Catalog && m.AuthCode != null)
                .OrderBy((ur, rm, m) => m.AuthCode)
                .Select((ur, rm, m) => m.AuthCode).ToListAsync(ct);
            return authCodeList.OfType<string>().Where(x => !x.IsNullOrEmpty()).ToList();
        }, ct), ct);
    }

    #endregion
}
