﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApeVolo.Business.Base;
using ApeVolo.Common.AttributeExt;
using ApeVolo.Common.Caches.Redis.Service;
using ApeVolo.Common.Exception;
using ApeVolo.Common.Extention;
using ApeVolo.Common.Global;
using ApeVolo.Common.Model;
using ApeVolo.Common.Resources;
using ApeVolo.Common.WebApp;
using ApeVolo.Entity.Permission.Role;
using ApeVolo.Entity.Permission.User;
using ApeVolo.IBusiness.Dto.Permission.Role;
using ApeVolo.IBusiness.ExportModel.Permission;
using ApeVolo.IBusiness.Interface.Permission.Department;
using ApeVolo.IBusiness.Interface.Permission.Menu;
using ApeVolo.IBusiness.Interface.Permission.Role;
using ApeVolo.IBusiness.Interface.Permission.User;
using ApeVolo.IBusiness.QueryModel;
using ApeVolo.IRepository.Permission.Role;
using AutoMapper;
using Castle.Core.Internal;
using SqlSugar;

namespace ApeVolo.Business.Permission.Role;

/// <summary>
/// 角色服务
/// </summary>
public class RoleService : BaseServices<Entity.Permission.Role.Role>, IRoleService
{
    #region 字段

    private readonly IMenuService _menuService;
    private readonly IRolesMenusService _rolesMenusService;
    private readonly IUserRolesService _userRolesService;
    private readonly IDepartmentService _departmentService;
    private readonly IRoleDeptService _roleDeptService;
    private readonly IRedisCacheService _redisCacheService;

    #endregion

    #region 构造函数

    public RoleService(IMapper mapper, IRoleRepository roleRepository, ICurrentUser currentUser,
        IMenuService menuService, IDepartmentService departmentService
        , IRolesMenusService rolesMenusService, IUserRolesService userRolesService,
        IRoleDeptService roleDeptService, IRedisCacheService redisCacheService)
    {
        Mapper = mapper;
        BaseDal = roleRepository;
        CurrentUser = currentUser;
        _menuService = menuService;
        _departmentService = departmentService;
        _rolesMenusService = rolesMenusService;
        _userRolesService = userRolesService;
        _roleDeptService = roleDeptService;
        _redisCacheService = redisCacheService;
    }

    #endregion

    #region 基础方法

    [UseTran]
    public async Task<bool> CreateAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        await VerificationUserRoleLevelAsync(createUpdateRoleDto.Level);
        if (await IsExistAsync(r => r.Name == createUpdateRoleDto.Name))
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Role"),
                createUpdateRoleDto.Name));
        }

        if (await IsExistAsync(r => r.Permission == createUpdateRoleDto.Permission))
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Role"),
                createUpdateRoleDto.Permission));
        }

        var role = Mapper.Map<Entity.Permission.Role.Role>(createUpdateRoleDto);
        await AddEntityAsync(role);

        if (!createUpdateRoleDto.Depts.IsNullOrEmpty() && createUpdateRoleDto.Depts.Count > 0)
        {
            var roleDepts = new List<RolesDepartments>();
            roleDepts.AddRange(createUpdateRoleDto.Depts.Select(rd => new RolesDepartments
                { RoleId = role.Id, DeptId = rd.Id }));
            await _roleDeptService.CreateAsync(roleDepts);
        }

        return true;
    }

    [UseTran]
    public async Task<bool> UpdateAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        //取出待更新数据
        var oldRole = await QueryFirstAsync(x => x.Id == createUpdateRoleDto.Id);
        if (oldRole.IsNull())
        {
            throw new BadRequestException(Localized.Get("DataNotExist"));
        }

        if (oldRole.Name != createUpdateRoleDto.Name && await IsExistAsync(x => x.Name == createUpdateRoleDto.Name))
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Role"),
                createUpdateRoleDto.Name));
        }

        if (oldRole.Permission != createUpdateRoleDto.Permission && await IsExistAsync(x =>
                x.Permission == createUpdateRoleDto.Permission))
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Role"),
                createUpdateRoleDto.Permission));
        }

        await VerificationUserRoleLevelAsync(createUpdateRoleDto.Level);
        var role = Mapper.Map<Entity.Permission.Role.Role>(createUpdateRoleDto);
        await UpdateEntityAsync(role);

        //删除部门权限关联
        await _roleDeptService.DeleteByRoleIdAsync(role.Id);
        if (!createUpdateRoleDto.Depts.IsNullOrEmpty() && createUpdateRoleDto.Depts.Count > 0)
        {
            var roleDepts = new List<RolesDepartments>();
            roleDepts.AddRange(createUpdateRoleDto.Depts.Select(rd => new RolesDepartments
                { RoleId = role.Id, DeptId = rd.Id }));
            await _roleDeptService.CreateAsync(roleDepts);
        }

        return true;
    }

    [UseTran]
    public async Task<bool> DeleteAsync(HashSet<long> ids)
    {
        //返回用户列表的最大角色等级
        var roles = await QueryByIdsAsync(ids);
        List<int> levels = new List<int>();
        levels.AddRange(roles.Select(x => x.Level).ToList());
        int minLevel = levels.Min();
        //验证当前用户角色等级是否大于待待删除的角色等级 ，不满足则抛异常
        await VerificationUserRoleLevelAsync(minLevel);

        //删除角色 角色部门 角色菜单
        await DeleteEntityListAsync(roles);
        return true;
    }

    public async Task<List<RoleDto>> QueryAsync(RoleQueryCriteria roleQueryCriteria, Pagination pagination)
    {
        Expression<Func<Entity.Permission.Role.Role, bool>> whereLambda = r => true;
        if (!roleQueryCriteria.RoleName.IsNullOrEmpty())
        {
            whereLambda = whereLambda.AndAlso(r =>
                r.Name.Contains(roleQueryCriteria.RoleName));
        }

        if (!roleQueryCriteria.CreateTime.IsNullOrEmpty())
        {
            whereLambda = whereLambda.AndAlso(r =>
                r.CreateTime >= roleQueryCriteria.CreateTime[0] && r.CreateTime <= roleQueryCriteria.CreateTime[1]);
        }

        var roleList = await BaseDal.QueryPageListAsync(whereLambda, pagination);
        foreach (var role in roleList)
        {
            //菜单
            var menus = Mapper.Map<List<Entity.Permission.Menu>>(await _menuService.FindByRoleIdAsync(role.Id));
            role.MenuList = menus;

            //部门
            var departments =
                Mapper.Map<List<Entity.Permission.Department>>(await _departmentService.QueryByRoleIdAsync(role.Id));
            role.DepartmentList = departments;
        }

        return Mapper.Map<List<RoleDto>>(roleList);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(RoleQueryCriteria roleQueryCriteria)
    {
        var roles = await QueryAsync(roleQueryCriteria, new Pagination { PageSize = 9999 });
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(roles.Select(x => new RoleExport()
        {
            Id = x.Id,
            Name = x.Name,
            Level = x.Level,
            Description = x.Description,
            DataScope = x.DataScope,
            DataDept = string.Join(",", x.DepartmentList.Select(d => d.Name).ToArray()),
            Permission = x.Permission,
            CreateTime = x.CreateTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    /// <summary>
    /// 获取用户全部角色
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<RoleSmallDto>> QueryByUserIdAsync(long id)
    {
        var roleSmallList =
            await BaseDal.QueryMuchAsync<Entity.Permission.Role.Role, UserRoles, Entity.Permission.Role.Role>(
                (r, ur) => new object[]
                {
                    JoinType.Left, r.Id == ur.RoleId
                },
                (r, ur) => r,
                (r, ur) => ur.UserId == id
            );

        return Mapper.Map<List<RoleSmallDto>>(roleSmallList);
    }

    public async Task<List<RoleDto>> QueryAllAsync()
    {
        var roleList = await BaseDal.QueryListAsync();
        foreach (var role in roleList)
        {
            //菜单
            var menus = Mapper.Map<List<Entity.Permission.Menu>>(await _menuService.FindByRoleIdAsync(role.Id));
            role.MenuList = menus;

            //部门
            var depts = Mapper.Map<List<Entity.Permission.Department>>(
                await _departmentService.QueryByRoleIdAsync(role.Id));
            role.DepartmentList = depts;
        }

        return Mapper.Map<List<RoleDto>>(roleList);
    }

    public async Task<int> QueryUserRoleLevelAsync(HashSet<long> ids)
    {
        List<int> levels = new List<int>();
        var roles = await BaseDal.QueryMuchAsync<Entity.Permission.Role.Role, UserRoles, Entity.Permission.Role.Role>(
            (r, ur) => new object[]
            {
                JoinType.Left, r.Id == ur.RoleId
            },
            (r, ur) => r,
            (r, ur) => ids.Contains(ur.UserId)
        );
        levels.AddRange(roles.Select(x => x.Level).ToList());
        int minLevel = levels.Min();
        return minLevel;
    }

    public async Task<int> VerificationUserRoleLevelAsync(int? level)
    {
        List<int> levels = new List<int>();
        var roles = await BaseDal.QueryMuchAsync<Entity.Permission.Role.Role, UserRoles, Entity.Permission.Role.Role>(
            (r, ur) => new object[]
            {
                JoinType.Left, r.Id == ur.RoleId
            },
            (r, ur) => r,
            (r, ur) => ur.UserId == CurrentUser.Id //"737368938475687938"//
        );
        levels.AddRange(roles.Select(x => x.Level).ToList());
        int minLevel = levels.Min();
        if (level != null && level < minLevel)
        {
            throw new BadRequestException("您无权修改或删除比你角色等级更高的数据！");
        }

        return minLevel;
    }


    [UseTran]
    public async Task<bool> UpdateRolesMenusAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        var role = await base.QuerySingleAsync(createUpdateRoleDto.Id);
        await VerificationUserRoleLevelAsync(role.Level);


        //删除菜单
        List<RoleMenu> roleMenus = new List<RoleMenu>();
        if (!createUpdateRoleDto.Menus.IsNullOrEmpty() && createUpdateRoleDto.Menus.Count > 0)
        {
            roleMenus.AddRange(createUpdateRoleDto.Menus.Select(rm => new RoleMenu
                { RoleId = role.Id, MenuId = rm.Id }));

            await _rolesMenusService.DeleteAsync(new List<long> { role.Id });
            await _rolesMenusService.CreateAsync(roleMenus);
        }

        //获取所有用户  删除缓存
        var userRoles = await _userRolesService.QueryByRoleIdsAsync(new HashSet<long> { role.Id });
        foreach (var ur in userRoles)
        {
            await _redisCacheService.RemoveAsync(RedisKey.UserPermissionById +
                                                 ur.UserId.ToString().ToMd5String16());
        }

        return true;
    }

    #endregion
}