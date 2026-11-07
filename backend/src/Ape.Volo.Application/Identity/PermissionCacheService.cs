using Ape.Volo.Application.Contracts.Identity.Models;
using System.Globalization;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Entity.Identity.Roles;
using Ape.Volo.Entity.Identity.Users;

namespace Ape.Volo.Application.Identity;

/// <summary>
/// 集中管理用户权限缓存的键、有效期和提交后清理。
/// </summary>
/// <param name="cache">既有缓存存储</param>
/// <param name="queries">角色和菜单关联查询</param>
/// <param name="unitOfWork">提交后动作登记</param>
/// <param name="options">既有缓存切面开关</param>
public sealed class PermissionCacheService(ICache cache, ISugarQueryProvider queries, IUnitOfWork unitOfWork,
    IOptions<AopOptions> options)
    : IPermissionCacheService
{
    private const string PolicyPrefix = "identity:";

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(PermissionCacheItem item, long userId, Func<Task<T>> load,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        // 事务内不回填可能尚未提交的结果。
        if (unitOfWork.IsInTransaction || (item != PermissionCacheItem.UserMenus && !options.Value.Cache))
            return await load();
        var key = Key(item, userId);
        return await cache.GetOrCreateAsync(key, load,
            TimeSpan.FromMinutes(item == PermissionCacheItem.UserMenus ? 120 : 60), ct);
    }

    /// <inheritdoc />
    public async Task UserChangedAsync(long userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await DataScopeChangedAsync(ct);
        await unitOfWork.AfterCommitAsync(() => ClearUserAsync(userId));
    }

    /// <inheritdoc />
    public Task DataScopeChangedAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return unitOfWork.AfterCommitAsync(async () =>
        {
            var users = await queries.QueryWithoutDataScopeAsync(db => db.Queryable<User>()
                .Select(user => user.Id).ToListAsync());
            foreach (var userId in users)
                await cache.RemoveAsync(Key(PermissionCacheItem.DataScope, userId));
        });
    }

    /// <inheritdoc />
    public async Task RoleChangedAsync(long roleId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await DataScopeChangedAsync(ct);
        await unitOfWork.AfterCommitAsync(async () =>
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
            await cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById.TrimEnd(':'));
        });
    }

    private async Task ClearUserAsync(long userId)
    {
        foreach (var item in Enum.GetValues<PermissionCacheItem>())
            await cache.RemoveAsync(Key(item, userId));
    }

    private static string Key(PermissionCacheItem item, long userId) =>
        PolicyPrefix + Prefix(item) + userId.ToString(CultureInfo.InvariantCulture);

    private static string Prefix(PermissionCacheItem item) => item switch
    {
        PermissionCacheItem.UserInfo => GlobalConstants.CachePrefix.UserInfoById,
        PermissionCacheItem.RoleCodes => GlobalConstants.CachePrefix.UserRoleCodes,
        PermissionCacheItem.AuthCodes => GlobalConstants.CachePrefix.UserAuthCodes,
        PermissionCacheItem.DataScope => GlobalConstants.CachePrefix.UserDataScopeById,
        PermissionCacheItem.UserMenus => GlobalConstants.CachePrefix.UserMenuById,
        _ => throw new ArgumentOutOfRangeException(nameof(item))
    };
}
