using Microsoft.AspNetCore.Authorization;
using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Identity.Menus.Models;
using Ape.Volo.Application.Contracts.Identity.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Common.Enums;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Identity;
using Ape.Volo.Entity.Identity.Roles;
using Ape.Volo.Entity.Identity.Users;
using SqlSugar;

namespace Ape.Volo.Application.Identity.Menus;

/// <summary>
/// 菜单服务
/// </summary>
public class MenuService : BaseService<Menu>, IMenuService
{
    #region 字段

    private readonly ISugarQueryProvider _sugarQueries;
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionCacheService _permissionCache;
    private readonly IApplicationAuthorizationService _authorization;

    #endregion

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="sugarQueries">不受数据权限限制的查询入口</param>
    /// <param name="currentUser">当前用户</param>
    /// <param name="unitOfWork">菜单写入事务</param>
    /// <param name="permissionCache">用户权限缓存入口</param>
    /// <param name="authorization">菜单权限定义变更检查</param>
    public MenuService(ISugarRepository<Menu> sugarRepository, IMapper mapper,
        ISugarQueryProvider sugarQueries, ICurrentUser currentUser, IUnitOfWork unitOfWork,
        IPermissionCacheService permissionCache, IApplicationAuthorizationService authorization)
        : base(sugarRepository, mapper)
    {
        _sugarQueries = sugarQueries;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _permissionCache = permissionCache;
        _authorization = authorization;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Menus.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateMenuDto createUpdateMenuDto, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(createUpdateMenuDto.AuthCode))
            await _authorization.CheckAsync([RoleCodes.Admin], [], ct);

        if (createUpdateMenuDto.MenuType is not (MenuType.Menu or MenuType.Button) &&
            !string.IsNullOrWhiteSpace(createUpdateMenuDto.AuthCode))
            return OperateResult.Error("目录、内嵌和外链菜单不能设置权限标识。");
        if (await TableWhere(m => m.Title == createUpdateMenuDto.Title).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateMenuDto.Title);
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink))
        {
            if (createUpdateMenuDto.AuthCode.IsNullOrEmpty())
            {
                return Required(() => createUpdateMenuDto.AuthCode);
            }
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink) &&
            await TableWhere(x => x.AuthCode == createUpdateMenuDto.AuthCode)
                .AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateMenuDto.AuthCode);
        }

        if (!createUpdateMenuDto.ComponentName.IsNullOrEmpty() && await TableWhere(m =>
                m.ComponentName == createUpdateMenuDto.ComponentName).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateMenuDto.ComponentName);
        }


        if (createUpdateMenuDto.MenuType is MenuType.InternalLink or MenuType.ExternalLink &&
            (!Uri.TryCreate(createUpdateMenuDto.Component, UriKind.Absolute, out var target) ||
             (target.Scheme != Uri.UriSchemeHttp && target.Scheme != Uri.UriSchemeHttps)))
        {
            return OperateResult.Error("内嵌与外链菜单的组件地址必须使用完整的 HTTP 或 HTTPS 地址");
        }


        var menu = Mapper.MapTo<Menu>(createUpdateMenuDto);

        await using var transaction = _unitOfWork.BeginTransaction(Propagation.Required);
        await AddAsync(menu, ct: ct);
        await transaction.CommitAsync(ct);
        return OperateResult.Success();
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Menus.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateMenuDto createUpdateMenuDto, CancellationToken ct = default)
    {
        //取出待更新数据
        var oldMenu = await TableWhere(x => x.Id == createUpdateMenuDto.Id).FirstAsync(ct);
        if (oldMenu.IsNull())
        {
            return NotFound(createUpdateMenuDto.Id);
        }

        if (!string.Equals(oldMenu.AuthCode ?? string.Empty, createUpdateMenuDto.AuthCode ?? string.Empty, StringComparison.Ordinal) ||
            (!string.IsNullOrWhiteSpace(oldMenu.AuthCode) &&
             (oldMenu.MenuType != createUpdateMenuDto.MenuType || oldMenu.Enabled != createUpdateMenuDto.Enabled)))
            await _authorization.CheckAsync([RoleCodes.Admin], [], ct);

        if (createUpdateMenuDto.MenuType is not (MenuType.Menu or MenuType.Button) &&
            !string.IsNullOrWhiteSpace(createUpdateMenuDto.AuthCode))
            return OperateResult.Error("目录、内嵌和外链菜单不能设置权限标识。");

        if (oldMenu.Title != createUpdateMenuDto.Title &&
            await TableWhere(x => x.Title == createUpdateMenuDto.Title).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateMenuDto.Title);
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink))
        {
            if (createUpdateMenuDto.AuthCode.IsNullOrEmpty())
            {
                return Required(() => createUpdateMenuDto.AuthCode);
            }
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink) &&
            await TableWhere(x => x.Id != createUpdateMenuDto.Id && x.AuthCode == createUpdateMenuDto.AuthCode)
                .AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateMenuDto.AuthCode);
        }

        if (!createUpdateMenuDto.ComponentName.IsNullOrEmpty())
        {
            if (oldMenu.ComponentName != createUpdateMenuDto.ComponentName &&
                await TableWhere(m => m.ComponentName == createUpdateMenuDto.ComponentName).AnyAsync(ct))
            {
                return AlreadyExists(() => createUpdateMenuDto.ComponentName);
            }
        }


        if (createUpdateMenuDto.MenuType is MenuType.InternalLink or MenuType.ExternalLink &&
            (!Uri.TryCreate(createUpdateMenuDto.Component, UriKind.Absolute, out var target) ||
             (target.Scheme != Uri.UriSchemeHttp && target.Scheme != Uri.UriSchemeHttps)))
        {
            return OperateResult.Error("内嵌与外链菜单的组件地址必须使用完整的 HTTP 或 HTTPS 地址");
        }


        var createUpdateMenu = Mapper.MapTo<Menu>(createUpdateMenuDto);
        await using var transaction = _unitOfWork.BeginTransaction(Propagation.Required);
        await UpdateAsync(createUpdateMenu, ct: ct);
        await _permissionCache.MenusChangedAsync([createUpdateMenu.Id], CancellationToken.None);
        await transaction.CommitAsync(ct);
        return OperateResult.Success();
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Menus.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var menuList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync(ct);
        if (menuList.Count < 1)
        {
            return NotFound();
        }

        var idList = new List<long>();
        foreach (var id in ids)
        {
            if (!idList.Contains(id))
            {
                idList.Add(id);
            }

            var menus = await TableWhere(m => m.ParentId == id).ToListAsync(ct);
            await FindChildIdsAsync(menus, idList, ct);
        }

        await using var transaction = _unitOfWork.BeginTransaction(Propagation.Required);
        var isTrue = await LogicDeleteAsync(x => idList.Contains(x.Id), ct);
        if (isTrue) await _permissionCache.MenusChangedAsync(idList, CancellationToken.None);
        await transaction.CommitAsync(ct);
        return OperateResult.Result(isTrue);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Menus.List)]
    public async Task<List<MenuVo>> GetListAsync(MenuQueryCriteria menuQueryCriteria, CancellationToken ct = default)
    {
        // 只查顶级菜单由 IsRoot 映射成 parent_id IS NULL，随通用条件一起生效
        var menus = await TableWhere(menuQueryCriteria.ApplyQueryConditionalModel(), x => x.Sort,
            OrderByType.Asc).ToListAsync(ct);
        var menuVos = Mapper.MapTo<List<MenuVo>>(menus);
        await FillHasChildrenAsync(menuVos, ct);
        return menuVos;
    }

    /// <summary>
    /// 按当前结果集的主键批量判定哪些行还有子节点。
    /// </summary>
    /// <param name="menuVos">当前结果集的菜单数据</param>
    /// <param name="ct">取消令牌</param>
    private async Task FillHasChildrenAsync(List<MenuVo> menuVos, CancellationToken ct)
    {
        if (menuVos.Count == 0)
        {
            return;
        }

        var ids = menuVos.Select(x => x.Id).ToList();
        var parentIds = await TableWhere(x => x.ParentId.HasValue && ids.Contains(x.ParentId.Value))
            .Select(x => x.ParentId).ToListAsync(ct);
        var parentIdSet = parentIds.Where(x => x.HasValue).Select(x => x!.Value).ToHashSet();
        foreach (var menuVo in menuVos)
        {
            menuVo.HasChildren = parentIdSet.Contains(menuVo.Id);
        }
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Menus.Download)]
    public async Task<List<ExportBase>> DownloadAsync(MenuQueryCriteria menuQueryCriteria, CancellationToken ct = default)
    {
        var menus = await TableWhere(menuQueryCriteria.ApplyQueryConditionalModel()).ToListAsync(ct);
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(menus.Select(x => new MenuExport
        {
            Id = x.Id,
            Title = x.Title,
            Path = x.Path!,
            AuthCode = x.AuthCode!,
            Component = x.Component!,
            ComponentName = x.ComponentName!,
            PId = x.ParentId,
            Sort = x.Sort,
            Icon = x.Icon!,
            MenuType = x.MenuType,
            KeepAlive = x.KeepAlive,
            Hidden = x.Hidden,
            CreationTime = x.CreationTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<List<MenuVo>> GetAllAsync(CancellationToken ct = default)
    {
        return Mapper.MapTo<List<MenuVo>>(await Table.ToListAsync(ct));
    }

    /// <inheritdoc />
    [Authorize]
    public async Task<List<MenuTreeVo>> BuildTreeAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.Id;

        // 用户可见菜单属于权限元数据，不受当前账号的创建范围限制。
        return await _permissionCache.GetOrCreateAsync(PermissionCacheItem.UserMenus, userId, () => _sugarQueries.QueryWithoutDataScopeAsync(async db =>
        {
            var menuList = await db
                .Queryable<UserRole, Role, RoleMenu, Menu>((ur, role, rm, m) =>
                    ur.RoleId == role.Id && role.Id == rm.RoleId && rm.MenuId == m.Id)
                .Where((ur, role, rm, m) => ur.UserId == userId && !role.IsDeleted && m.Enabled && m.MenuType != MenuType.Button)
                .OrderBy((ur, role, rm, m) => m.Sort)
                .Select((ur, role, rm, m) => new MenuVo
                {
                    Title = m.Title,
                    Path = m.Path!,
                    AuthCode = m.AuthCode!,
                    Component = m.Component!,
                    ComponentName = m.ComponentName!,
                    ParentId = m.ParentId,
                    Sort = m.Sort,
                    Icon = m.Icon!,
                    MenuType = m.MenuType,
                    IsDeleted = m.IsDeleted,
                    Id = m.Id,
                    CreationTime = m.CreationTime,
                    CreatorId = m.CreatorId,
                    KeepAlive = m.KeepAlive,
                    Hidden = m.Hidden
                }).Distinct().ToListAsync(ct);
            var menuListChild = TreeHelper<MenuVo>.ListToTrees(menuList, "Id", "ParentId", null);
            return await BuildAsync(menuListChild, ct);
        }, ct), ct);
    }

    /// <summary>
    /// 构建前端路由菜单
    /// </summary>
    /// <param name="menuList"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    private static async Task<List<MenuTreeVo>> BuildAsync(List<MenuVo> menuList,
        CancellationToken ct = default)
    {
        List<MenuTreeVo> menuVos = new List<MenuTreeVo>();
        foreach (var menu in menuList)
        {
            ct.ThrowIfCancellationRequested();
            List<MenuVo> menuDtoList = menu.Children ?? [];
            MenuTreeVo menuVo = new MenuTreeVo
            {
                Name = menu.ComponentName.IsNullOrEmpty() ? menu.Title : menu.ComponentName,
                Path = menu.Path,
                Hidden = menu.Hidden,
                MenuType = menu.MenuType
            };

            if (menu.ParentId is null)
            {
                menuVo.Component = menu.Component.IsNullOrEmpty() ? "Layout" : menu.Component;
            }
            else if (!menu.Component.IsNullOrEmpty())
            {
                menuVo.Component = menu.Component;
            }

            menuVo.Meta = new MenuMetaVo(menu.Title, menu.Icon ?? string.Empty, menu.KeepAlive);
            if (menuDtoList is { Count: > 0 })
            {
                menuVo.AlwaysShow = true;
                menuVo.Redirect = "noredirect";
                menuVo.Children = await BuildAsync(menuDtoList, ct);
            }

            menuVos.Add(menuVo);
        }

        return menuVos;
    }

    /// <summary>
    /// 获取所有下级菜单
    /// </summary>
    /// <param name="menuList"></param>
    /// <param name="ids"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    private async Task FindChildIdsAsync(List<Menu> menuList, List<long> ids,
        CancellationToken ct = default)
    {
        if (menuList is { Count: > 0 })
        {
            foreach (var menu in menuList)
            {
                if (!ids.Contains(menu.Id))
                {
                    ids.Add(menu.Id);
                }

                List<Menu> menus = await TableWhere(m => m.ParentId == menu.Id).ToListAsync(ct);
                if (menus is { Count: > 0 })
                {
                    await FindChildIdsAsync(menus, ids, ct);
                }
            }
        }

        await Task.FromResult(ids);
    }

    #endregion

}
