using Ape.Volo.Application.Contracts.Identity.Models;

namespace Ape.Volo.Application.Contracts.Identity;

/// <summary>
/// 用户权限缓存的内部入口，不生成 HTTP 端点。
/// </summary>
public interface IPermissionCacheService
{
    /// <summary>
    /// 按用户读取缓存，未命中时加载；与失效操作协调，防止旧结果重新回填。
    /// </summary>
    /// <typeparam name="T">结果类型</typeparam>
    /// <param name="item">缓存内容</param>
    /// <param name="userId">用户 ID</param>
    /// <param name="load">未命中时的数据读取</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>缓存或加载结果</returns>
    Task<T> GetOrCreateAsync<T>(PermissionCacheItem item, long userId, Func<Task<T>> load,
        CancellationToken ct = default);

    /// <summary>用户变更提交后清理该用户的资料、角色、权限、菜单和数据范围缓存。</summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    Task UserChangedAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 用户归属或部门树变更提交后，使所有依赖组织成员关系的数据范围缓存失效。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task DataScopeChangedAsync(CancellationToken ct = default);

    /// <summary>角色变更提交后清理关联用户的权限缓存。</summary>
    /// <param name="roleId">角色 ID</param>
    /// <param name="ct">取消令牌</param>
    Task RoleChangedAsync(long roleId, CancellationToken ct = default);

    /// <summary>菜单变更提交后清理关联用户的权限缓存。</summary>
    /// <param name="menuIds">变更的菜单 ID</param>
    /// <param name="ct">取消令牌</param>
    Task MenusChangedAsync(IEnumerable<long> menuIds, CancellationToken ct = default);
}
