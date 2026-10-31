namespace Ape.Volo.Application.Contracts.Permission.Models;

/// <summary>
/// 用户权限相关缓存项，沿用现有缓存键和有效期。
/// </summary>
public enum PermissionCacheItem
{
    /// <summary>用户资料。</summary>
    UserInfo,
    /// <summary>用户角色代码。</summary>
    RoleCodes,
    /// <summary>用户权限代码。</summary>
    AuthCodes,
    /// <summary>数据范围用户集合。</summary>
    DataScopeUsers,
    /// <summary>全部数据范围标识。</summary>
    AllDataScope,
    /// <summary>用户路由菜单。</summary>
    UserMenus
}
