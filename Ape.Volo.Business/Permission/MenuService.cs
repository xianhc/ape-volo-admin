using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.Entity.Core.Permission.Role;
using Ape.Volo.Entity.Core.Permission.User;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.Menu;
using Ape.Volo.ViewModel.Report.Permission;
using SqlSugar;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 菜单服务
/// </summary>
public class MenuService : BaseServices<Menu>, IMenuService
{
    #region 基础方法

    /// <summary>
    /// 新增菜单
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateMenuDto createUpdateMenuDto)
    {
        if (await TableWhere(m => m.Title == createUpdateMenuDto.Title).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateMenuDto,
                nameof(createUpdateMenuDto.Title)));
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink))
        {
            if (createUpdateMenuDto.AuthCode.IsNullOrEmpty())
            {
                return OperateResult.Error(ValidationError.Required(createUpdateMenuDto,
                    nameof(createUpdateMenuDto.AuthCode)));
            }
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink) &&
            await TableWhere(x => x.AuthCode == createUpdateMenuDto.AuthCode)
                .AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateMenuDto,
                nameof(createUpdateMenuDto.AuthCode)));
        }

        if (!createUpdateMenuDto.ComponentName.IsNullOrEmpty() && await TableWhere(m =>
                m.ComponentName == createUpdateMenuDto.ComponentName).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateMenuDto,
                nameof(createUpdateMenuDto.ComponentName)));
        }


        if (createUpdateMenuDto.MenuType == MenuType.ExternalLink)
        {
            string http = "http://", https = "https://";
            if (!(createUpdateMenuDto.Path.ToLower().StartsWith(http) ||
                  createUpdateMenuDto.Path.ToLower().StartsWith(https)))
            {
                return OperateResult.Error("External link menus must start with http:// or https://");
            }
        }


        var menu = App.Mapper.MapTo<Menu>(createUpdateMenuDto);

        await AddAsync(menu);
        if (menu.ParentId > 0)
        {
            //清理缓存
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadMenusByPId +
                                        menu.ParentId.ToString().ToMd5String16());
            var tempMenu = await TableWhere(x => x.Id == menu.ParentId).FirstAsync();
            if (tempMenu.IsNotNull())
            {
                var count = await TableWhere(x => x.ParentId == tempMenu.Id).CountAsync();
                tempMenu.SubCount = count;
                await UpdateAsync(tempMenu);
            }
        }

        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadAllMenu);
        return OperateResult.Success();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateMenuDto createUpdateMenuDto)
    {
        //取出待更新数据
        var oldMenu = await TableWhere(x => x.Id == createUpdateMenuDto.Id).FirstAsync();
        if (oldMenu.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateMenuDto, LanguageKeyConstants.Menu,
                nameof(createUpdateMenuDto.Id)));
        }

        if (oldMenu.Title != createUpdateMenuDto.Title &&
            await TableWhere(x => x.Title == createUpdateMenuDto.Title).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateMenuDto,
                nameof(createUpdateMenuDto.Title)));
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink))
        {
            if (createUpdateMenuDto.AuthCode.IsNullOrEmpty())
            {
                return OperateResult.Error(ValidationError.Required(createUpdateMenuDto,
                    nameof(createUpdateMenuDto.AuthCode)));
            }
        }

        if (createUpdateMenuDto.MenuType is not (MenuType.Catalog or MenuType.InternalLink or MenuType.ExternalLink) &&
            await TableWhere(x => x.AuthCode == createUpdateMenuDto.AuthCode)
                .AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateMenuDto,
                nameof(createUpdateMenuDto.AuthCode)));
        }

        if (!createUpdateMenuDto.ComponentName.IsNullOrEmpty())
        {
            if (oldMenu.ComponentName != createUpdateMenuDto.ComponentName &&
                await TableWhere(m => m.ComponentName.Equals(createUpdateMenuDto.ComponentName)).AnyAsync())
            {
                return OperateResult.Error(ValidationError.IsExist(createUpdateMenuDto,
                    nameof(createUpdateMenuDto.ComponentName)));
            }
        }


        if (createUpdateMenuDto.MenuType == MenuType.ExternalLink)
        {
            string http = "http://", https = "https://";
            if (!(createUpdateMenuDto.Path.ToLower().StartsWith(http) ||
                  createUpdateMenuDto.Path.ToLower().StartsWith(https)))
            {
                return OperateResult.Error("External link menus must start with http:// or https://");
            }
        }


        var createUpdateMenu = App.Mapper.MapTo<Menu>(createUpdateMenuDto);
        await UpdateAsync(createUpdateMenu);
        //清理缓存
        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadMenusById +
                                    createUpdateMenu.Id.ToString().ToMd5String16());
        if (createUpdateMenu.ParentId > 0)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadMenusByPId +
                                        createUpdateMenu.ParentId.ToString().ToMd5String16());
        }

        //重新计算子节点个数
        if (oldMenu.ParentId != createUpdateMenu.ParentId)
        {
            if (createUpdateMenu.ParentId > 0)
            {
                var tmpMenu = await TableWhere(x => x.Id == createUpdateMenu.ParentId).FirstAsync();
                if (tmpMenu.IsNotNull())
                {
                    var count = await TableWhere(x => x.ParentId == tmpMenu.Id).CountAsync();
                    tmpMenu.SubCount = count;
                    await UpdateAsync(tmpMenu, x => x.SubCount);
                }

                if (oldMenu.ParentId > 0)
                {
                    var tmpMenu2 = await TableWhere(x => x.Id == oldMenu.ParentId).FirstAsync();
                    if (tmpMenu2.IsNotNull())
                    {
                        var count = await TableWhere(x => x.ParentId == tmpMenu2.Id).CountAsync();
                        tmpMenu2.SubCount = count;
                        await UpdateAsync(tmpMenu2, x => x.SubCount);
                    }
                }
            }
        }

        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadAllMenu);
        return OperateResult.Success();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids)
    {
        var menuList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        if (menuList.Count < 1)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        var idList = new List<long>();
        foreach (var id in ids)
        {
            if (!idList.Contains(id))
            {
                idList.Add(id);
            }

            var menus = await TableWhere(m => m.ParentId == id).ToListAsync();
            await FindChildIdsAsync(menus, idList);
        }


        var pIds = menuList.Select(x => x.ParentId);

        var updateMenuList = await TableWhere(x => pIds.Contains(x.Id)).ToListAsync();

        var isTrue = await LogicDelete<Menu>(x => idList.Contains(x.Id));

        if (isTrue)
        {
            if (updateMenuList.Any())
            {
                foreach (var m in updateMenuList)
                {
                    var count = await SugarClient.Queryable<Menu>().Where(x => x.ParentId == m.Id)
                        .CountAsync();
                    m.SubCount = count;
                }

                isTrue = await UpdateAsync(updateMenuList, x => x.SubCount);
            }

            if (isTrue)
            {
                //清除缓存
                foreach (var id in idList)
                {
                    await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadMenusById +
                                                id.ToString().ToMd5String16());
                    await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadMenusByPId +
                                                id.ToString().ToMd5String16());
                }
            }
        }

        await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.LoadAllMenu);
        return OperateResult.Result(isTrue);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="menuQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<MenuVo>> QueryAsync(MenuQueryCriteria menuQueryCriteria)
    {
        var menus = await TableWhere(menuQueryCriteria.ApplyQueryConditionalModel(), null, x => x.Sort, OrderByType.Asc)
            .ToListAsync();
        var menuVos = App.Mapper.MapTo<List<MenuVo>>(menus);
        return menuVos;
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="menuQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(MenuQueryCriteria menuQueryCriteria)
    {
        var menus = await TableWhere(menuQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(menus.Select(x => new MenuExport
        {
            Id = x.Id,
            Title = x.Title,
            Path = x.Path,
            AuthCode = x.AuthCode,
            Component = x.Component,
            ComponentName = x.ComponentName,
            PId = 0,
            Sort = x.Sort,
            Icon = x.Icon,
            MenuType = x.MenuType,
            KeepAlive = x.KeepAlive,
            Hidden = x.Hidden,
            SubCount = x.SubCount,
            CreateTime = x.CreateTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <returns></returns>
    public async Task<List<MenuVo>> QueryAllAsync()
    {
        var menuVos = await App.Cache.GetAsync<List<MenuVo>>(GlobalConstants.CachePrefix.LoadAllMenu);
        if (menuVos != null && menuVos.Count != 0)
        {
            return menuVos;
        }

        menuVos = App.Mapper.MapTo<List<MenuVo>>(await Table.ToListAsync());
        if (menuVos.Count != 0)
        {
            await App.Cache.SetAsync(GlobalConstants.CachePrefix.LoadAllMenu, menuVos,
                TimeSpan.FromSeconds(120), null);
        }

        return menuVos;
    }

    /// <summary>
    /// 构建前端路由菜单
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns></returns>
    [UseCache(Expiration = 120, KeyPrefix = GlobalConstants.CachePrefix.UserMenuById)]
    public async Task<List<MenuTreeVo>> BuildTreeAsync(long userId)
    {
        var menuList = await SugarClient
            .Queryable<UserRole, RoleMenu, Menu>((ur, rm, m) => ur.RoleId == rm.RoleId && rm.MenuId == m.Id)
            .Where((ur, rm, m) => ur.UserId == userId && m.MenuType != MenuType.Button)
            .OrderBy((ur, rm, m) => m.Sort)
            .ClearFilter<ICreateByEntity>()
            .Select((ur, rm, m) => new MenuVo
            {
                Title = m.Title,
                Path = m.Path,
                AuthCode = m.AuthCode,
                Component = m.Component,
                ComponentName = m.ComponentName,
                ParentId = m.ParentId,
                Sort = m.Sort,
                Icon = m.Icon,
                MenuType = m.MenuType,
                IsDeleted = m.IsDeleted,
                Id = m.Id,
                CreateTime = m.CreateTime,
                CreateBy = m.CreateBy,
                KeepAlive = m.KeepAlive,
                Hidden = m.Hidden
            }).Distinct().ToListAsync();
        var menuListChild = TreeHelper<MenuVo>.ListToTrees(menuList, "Id", "ParentId", 0);
        return await BuildAsync(menuListChild);
    }

    /// <summary>
    /// 查询同级与父级菜单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadMenusById)]
    public async Task<List<MenuVo>> FindSuperiorAsync(long id)
    {
        Expression<Func<Menu, bool>> whereLambda = m => true;
        var menu = await TableWhere(x => x.Id == id).SingleAsync();
        List<MenuVo> menuDtoList;
        if (menu.ParentId == 0)
        {
            var menus = await TableWhere(x => x.ParentId == 0, null, x => x.Sort).ToListAsync();
            menuDtoList = App.Mapper.MapTo<List<MenuVo>>(menus);
            menuDtoList.ForEach(x => x.Children = null);
        }
        else
        {
            //查出同级菜单ID
            List<long> parentIds = new List<long>();
            parentIds.Add(menu.ParentId);
            await GetParentIdsAsync(menu, parentIds);
            whereLambda =
                whereLambda.AndAlso(m => parentIds.Contains(Convert.ToInt64(m.ParentId)) || m.ParentId == 0);

            //可以优化语句
            var menus = await TableWhere(whereLambda, null, x => x.Sort).ToListAsync();
            var allMenu = await Table.ToListAsync();
            foreach (var m in menus)
            {
                if (parentIds.Contains(m.Id) && m.ParentId == 0)
                {
                    m.Children = allMenu.Where(x => x.ParentId == m.Id).ToList();
                }
            }


            var tempDtos = App.Mapper.MapTo<List<MenuVo>>(menus);
            menuDtoList = TreeHelper<MenuVo>.ListToTrees(tempDtos, "Id", "ParentId", 0);
        }

        return menuDtoList;
    }

    /// <summary>
    /// 构建前端路由菜单
    /// </summary>
    /// <param name="menuList"></param>
    /// <returns></returns>
    private static async Task<List<MenuTreeVo>> BuildAsync(List<MenuVo> menuList)
    {
        List<MenuTreeVo> menuVos = new List<MenuTreeVo>();
        foreach (var menu in menuList)
        {
            List<MenuVo> menuDtoList = menu.Children;
            MenuTreeVo menuVo = new MenuTreeVo
            {
                Name = menu.ComponentName.IsNullOrEmpty() ? menu.Title : menu.ComponentName,
                Path = menu.Path,
                Hidden = menu.Hidden,
                MenuType = menu.MenuType
            };

            if (menu.ParentId == 0)
            {
                menuVo.Component = menu.Component.IsNullOrEmpty() ? "Layout" : menu.Component;
            }
            else if (!menu.Component.IsNullOrEmpty())
            {
                menuVo.Component = menu.Component;
            }

            menuVo.Meta = new MenuMetaVo(menu.Title, menu.Icon, menu.KeepAlive);
            if (menuDtoList is { Count: > 0 })
            {
                menuVo.AlwaysShow = true;
                menuVo.Redirect = "noredirect";
                menuVo.Children = await BuildAsync(menuDtoList);
            }

            menuVos.Add(menuVo);
        }

        return menuVos;
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="pid">父Id</param>
    /// <returns></returns>
    [UseCache(Expiration = 30, KeyPrefix = GlobalConstants.CachePrefix.LoadMenusByPId)]
    public async Task<List<MenuVo>> FindByPIdAsync(long pid = 0)
    {
        List<MenuVo> menuVos = App.Mapper.MapTo<List<MenuVo>>(await TableWhere(x => x.ParentId == pid, null,
            o => o.Sort).ToListAsync());
        foreach (var item in menuVos)
        {
            item.Children = null;
        }

        return menuVos;
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="m"></param>
    /// <param name="parentIds">父id</param>
    /// <returns></returns>
    private async Task<List<long>> GetParentIdsAsync(Menu m, List<long> parentIds)
    {
        var menu = await TableWhere(x => x.Id == m.ParentId).FirstAsync();
        if (menu.IsNull() || menu.ParentId == 0)
        {
            //parentIds.Add(menu.PId);
            return await Task.FromResult(parentIds);
        }

        parentIds.Add(menu.ParentId);
        return await GetParentIdsAsync(menu, parentIds);
    }

    /// <summary>
    /// 获取所有下级菜单
    /// </summary>
    /// <param name="menuList"></param>
    /// <param name="ids"></param>
    /// <returns></returns>
    private async Task FindChildIdsAsync(List<Menu> menuList, List<long> ids)
    {
        if (menuList is { Count: > 0 })
        {
            foreach (var menu in menuList)
            {
                if (!ids.Contains(menu.Id))
                {
                    ids.Add(menu.Id);
                }

                List<Menu> menus = await TableWhere(m => m.ParentId == menu.Id).ToListAsync();
                if (menus is { Count: > 0 })
                {
                    await FindChildIdsAsync(menus, ids);
                }
            }
        }

        await Task.FromResult(ids);
    }


    /// <summary>
    /// 查询子级
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<long>> FindChildAsync(long id)
    {
        List<long> ids = new List<long> { id };
        var menus = await QueryAllAsync();
        if (menus.Count > 0)
        {
            var menus2 = menus.Where(x => x.ParentId == id).ToList();
            if (menus2.Count > 0)
            {
                var ids2 = menus2.Select(x => x.Id).ToList();
                ids.AddRange(ids2);

                var menus3 = menus.Where(x => ids2.Contains(Convert.ToInt64(x.ParentId))).ToList();
                if (menus3.Count > 0)
                {
                    ids.AddRange(menus3.Select(x => x.Id).ToList());
                }
            }

            return ids;
        }

        return new List<long>();
    }

    #endregion
}
