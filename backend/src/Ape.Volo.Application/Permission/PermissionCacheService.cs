using Ape.Volo.Application.Contracts.Permission.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Entity.Permission.Users;

namespace Ape.Volo.Application.Permission;

/// <summary>
/// 集中管理既有用户权限缓存的键、有效期和提交后清理，不改变权限查询规则。
/// </summary>
/// <param name="cache">既有缓存存储</param>
/// <param name="queries">角色和菜单关联查询</param>
/// <param name="unitOfWork">提交后动作登记</param>
/// <param name="options">既有缓存切面开关</param>
public sealed class PermissionCacheService(ICache cache, ISugarQueryProvider queries, IUnitOfWork unitOfWork,
    IOptions<AopOptions> options)
    : IPermissionCacheService
{
    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(PermissionCacheItem item, long userId, Func<Task<T>> load,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        // 事务内不回填可能尚未提交的结果。
        if (unitOfWork.IsInTransaction || (item != PermissionCacheItem.UserMenus && !options.Value.Cache))
            return await load();
        var key = Prefix(item) + userId.ToString().ToMd5String16();
        return await cache.GetOrCreateAsync(key, load,
            TimeSpan.FromMinutes(item == PermissionCacheItem.UserMenus ? 120 : 60), ct);
    }

    /// <inheritdoc />
    public Task UserChangedAsync(long userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return unitOfWork.AfterCommitAsync(() => ClearUserAsync(userId));
    }

    /// <inheritdoc />
    public Task RoleChangedAsync(long roleId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return unitOfWork.AfterCommitAsync(async () =>
        {
            var users = await queries.QueryWithoutDataScopeAsync(db => db.Queryable<UserRole>()
                .Where(link => link.RoleId == roleId).Select(link => link.UserId).Distinct().ToListAsync());
            foreach (var userId in users) await ClearUserAsync(userId);
        });
    }

    /// <inheritdoc />
    public Task MenusChangedAsync(IEnumerable<long> menuIds, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var ids = menuIds.Distinct().ToArray();
        return unitOfWork.AfterCommitAsync(async () =>
        {
            var users = await queries.QueryWithoutDataScopeAsync(db => db.Queryable<UserRole, RoleMenu>((ur, rm) => ur.RoleId == rm.RoleId)
                .Where((ur, rm) => ids.Contains(rm.MenuId)).Select((ur, rm) => ur.UserId).Distinct().ToListAsync());
            foreach (var userId in users) await ClearUserAsync(userId);
            await cache.InvalidateAsync(GlobalConstants.CachePrefix.UserMenuById.TrimEnd(':'));
        });
    }

    private async Task ClearUserAsync(long userId)
    {
        var suffix = userId.ToString().ToMd5String16();
        foreach (var item in Enum.GetValues<PermissionCacheItem>())
            await cache.InvalidateAsync(Prefix(item) + suffix);
    }

    private static string Prefix(PermissionCacheItem item) => item switch
    {
        PermissionCacheItem.UserInfo => GlobalConstants.CachePrefix.UserInfoById,
        PermissionCacheItem.RoleCodes => GlobalConstants.CachePrefix.UserRoleCodes,
        PermissionCacheItem.AuthCodes => GlobalConstants.CachePrefix.UserAuthCodes,
        PermissionCacheItem.DataScopeUsers => GlobalConstants.CachePrefix.UserDataScopeById,
        PermissionCacheItem.AllDataScope => GlobalConstants.CachePrefix.UserDataScopeAllById,
        PermissionCacheItem.UserMenus => GlobalConstants.CachePrefix.UserMenuById,
        _ => throw new ArgumentOutOfRangeException(nameof(item))
    };
}
