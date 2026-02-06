using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.Permission.Role;
using Ape.Volo.Entity.Core.Permission.User;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission.Role;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.Role;
using Ape.Volo.ViewModel.Report.Permission;
using SqlSugar;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 角色服务
/// </summary>
public class RoleService : BaseServices<Role>, IRoleService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> CreateAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        await VerificationUserRoleLevelAsync(createUpdateRoleDto.Level);
        if (await TableWhere(r => r.Name == createUpdateRoleDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.Name)));
        }

        if (await TableWhere(r => r.AuthCode == createUpdateRoleDto.AuthCode).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.AuthCode)));
        }

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize && createUpdateRoleDto.DeptIdArray.Count == 0)
        {
            return OperateResult.Error(App.L.R("{0}AtLeastOne",
                App.L.R("Sys.Department")));
        }

        var role = App.Mapper.MapTo<Role>(createUpdateRoleDto);
        await AddAsync(role);

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize)
        {
            var roleDepts = new List<RoleDepartment>();
            roleDepts.AddRange(createUpdateRoleDto.DeptIdArray.Select(d => new RoleDepartment
            { RoleId = role.Id, DeptId = d }));
            await SugarClient.Insertable(roleDepts).ExecuteCommandAsync();
        }

        return OperateResult.Success();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateAsync(CreateUpdateRoleDto createUpdateRoleDto)
    {
        //取出待更新数据
        var oldRole = await TableWhere(x => x.Id == createUpdateRoleDto.Id).Includes(x => x.Users).FirstAsync();
        if (oldRole.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateRoleDto, LanguageKeyConstants.Role,
                nameof(createUpdateRoleDto.Id)));
        }

        if (oldRole.Name != createUpdateRoleDto.Name &&
            await TableWhere(x => x.Name == createUpdateRoleDto.Name).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.Name)));
        }

        if (oldRole.AuthCode != createUpdateRoleDto.AuthCode &&
            await TableWhere(x => x.AuthCode == createUpdateRoleDto.AuthCode).AnyAsync())
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.AuthCode)));
        }

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize && createUpdateRoleDto.DeptIdArray.Count == 0)
        {
            return OperateResult.Error(App.L.R("{0}AtLeastOne",
                App.L.R("Sys.Department")));
        }


        await VerificationUserRoleLevelAsync(createUpdateRoleDto.Level);
        var role = App.Mapper.MapTo<Role>(createUpdateRoleDto);
        await UpdateAsync(role);

        //删除部门权限关联
        await SugarClient.Deleteable<RoleDepartment>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync();
        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize)
        {
            var roleDepts = new List<RoleDepartment>();
            roleDepts.AddRange(createUpdateRoleDto.DeptIdArray.Select(d => new RoleDepartment
            { RoleId = role.Id, DeptId = d }));
            await SugarClient.Insertable(roleDepts).ExecuteCommandAsync();
        }

        foreach (var user in oldRole.Users)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserDataScopeById +
                                        user.Id.ToString().ToMd5String16());
        }

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
        //返回用户列表的最大角色等级
        var roles = await TableWhere(x => ids.Contains(x.Id)).Includes(x => x.Users).ToListAsync();
        if (roles.Count == 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        int userCount = 0;
        if (roles.Any(role => role.Users != null && role.Users.Count != 0))
        {
            userCount++;
        }

        if (userCount > 0)
        {
            return OperateResult.Error(ValidationError.DataAssociationExists());
        }


        List<int> levels = new List<int>();
        levels.AddRange(roles.Select(x => x.Level).ToList());
        int minLevel = levels.Min();
        //验证当前用户角色等级是否大于待待删除的角色等级 ，不满足则抛异常
        await VerificationUserRoleLevelAsync(minLevel);

        //删除角色 角色部门 角色菜单
        var result = await LogicDelete<Role>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<RoleVo>> QueryAsync(RoleQueryCriteria roleQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<Role>
        {
            Pagination = pagination,
            ConditionalModels = roleQueryCriteria.ApplyQueryConditionalModel(),
            IsIncludes = true,
            IgnorePropertyNameList = new[] { "Users", "MenuList", "ApiList" }
        };
        var roleList =
            await TablePageAsync(queryOptions);

        return App.Mapper.MapTo<List<RoleVo>>(roleList);
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(RoleQueryCriteria roleQueryCriteria)
    {
        var roles = await TableWhere(roleQueryCriteria.ApplyQueryConditionalModel()).Includes(x => x.DepartmentList)
            .ToListAsync();
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(roles.Select(x => new RoleExport
        {
            Id = x.Id,
            Name = x.Name,
            Level = x.Level,
            Description = x.Description,
            DataScopeType = x.DataScopeType,
            DataDept = string.Join(",", x.DepartmentList.Select(d => d.Name).ToArray()),
            AuthCode = x.AuthCode,
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
    public async Task<List<RoleVo>> QueryAllAsync()
    {
        var roleList = await Table.Includes(x => x.MenuList).Includes(x => x.DepartmentList).ToListAsync();

        return App.Mapper.MapTo<List<RoleVo>>(roleList);
    }

    /// <summary>
    /// 查询角色等级
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<int?> QueryUserRoleLevelAsync(HashSet<long> ids)
    {
        var levels = await SugarClient.Queryable<Role, UserRole>((r, ur) => new JoinQueryInfos(
                JoinType.Left, r.Id == ur.RoleId
            )).Where((r, ur) => ids.Contains(ur.UserId))
            .Select((r) => r.Level).ToListAsync();
        if (levels.Any())
        {
            var minLevel = levels.Min();
            return minLevel;
        }

        return null;
    }

    /// <summary>
    /// 验证用户角色等级
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<int> VerificationUserRoleLevelAsync(int? level)
    {
        var minLevel = 999;
        var levels = await SugarClient.Queryable<Role, UserRole>((r, ur) => new JoinQueryInfos(
                JoinType.Left, r.Id == ur.RoleId
            )).Where((r, ur) => ur.UserId == App.HttpUser.Id)
            .Select((r) => r.Level).ToListAsync();

        if (levels.Any())
        {
            minLevel = levels.Min();
        }

        if (level != null && level < minLevel)
        {
            throw new BadRequestException(
                App.L.R("Error.PermissionDenied.HigherRoleData"));
        }

        return minLevel;
    }

    /// <summary>
    /// 更新角色菜单
    /// </summary>
    /// <param name="updateRoleMenuDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateRoleMenuAsync(UpdateRoleMenuDto updateRoleMenuDto)
    {
        var role = await TableWhere(x => x.Id == updateRoleMenuDto.Id).Includes(x => x.Users).FirstAsync();
        if (role.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        await VerificationUserRoleLevelAsync(role.Level);


        //删除菜单
        List<RoleMenu> roleMenus = new List<RoleMenu>();
        roleMenus.AddRange(updateRoleMenuDto.MenuIdArray.Select(m => new RoleMenu
        { RoleId = role.Id, MenuId = m }));

        await SugarClient.Deleteable<RoleMenu>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync();
        await SugarClient.Insertable(roleMenus).ExecuteCommandAsync();


        //删除用户缓存
        foreach (var user in role.Users)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserAuthCodes +
                                        user.Id.ToString().ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                        user.Id.ToString().ToMd5String16());
        }

        return OperateResult.Success();
    }

    /// <summary>
    /// 更新角色Api路由
    /// </summary>
    /// <param name="updateRoleApiDto"></param>
    /// <returns></returns>
    [UseTran]
    public async Task<OperateResult> UpdateRoleApiAsync(UpdateRoleApiDto updateRoleApiDto)
    {
        var role = await TableWhere(x => x.Id == updateRoleApiDto.Id).Includes(x => x.Users).FirstAsync();
        if (role.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        await VerificationUserRoleLevelAsync(role.Level);


        //删除菜单
        List<RoleApis> roleApis = new List<RoleApis>();
        // 这里过滤一下自生成的一级节点ID
        updateRoleApiDto.ApiIdArray = updateRoleApiDto.ApiIdArray.Where(x => x > 10000).ToList();
        roleApis.AddRange(updateRoleApiDto.ApiIdArray.Select(a => new RoleApis
        { RoleId = role.Id, ApisId = a }));

        await SugarClient.Deleteable<RoleApis>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync();
        await SugarClient.Insertable(roleApis).ExecuteCommandAsync();


        //删除用户缓存
        foreach (var user in role.Users)
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserAuthUrls +
                                        user.Id.ToString().ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                        user.Id.ToString().ToMd5String16());
        }

        return OperateResult.Success();
    }

    #endregion
}
