using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Permission.Roles.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Entity.Permission.Users;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Permission.Roles;

/// <summary>
/// 角色服务
/// </summary>
public class RoleService : BaseService<Role>, IRoleService
{
    private readonly IPermissionCacheService _permissionCache;

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="permissionCache">权限缓存入口</param>
    public RoleService(ISugarRepository<Role> sugarRepository, IMapper mapper, IPermissionCacheService permissionCache) : base(sugarRepository, mapper)
    {
        _permissionCache = permissionCache;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Roles.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateRoleDto createUpdateRoleDto, CancellationToken ct = default)
    {
        if (await TableWhere(r => r.Name == createUpdateRoleDto.Name).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.Name)));
        }

        if (await TableWhere(r => r.AuthCode == createUpdateRoleDto.AuthCode).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.AuthCode)));
        }

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize && createUpdateRoleDto.DeptIdArray.Count == 0)
        {
            return OperateResult.Error("部门 集合必须至少包含一个项目");
        }

        var role = Mapper.MapTo<Role>(createUpdateRoleDto);
        await AddAsync(role, ct: ct);

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize)
        {
            var roleDepts = new List<RoleDepartment>();
            roleDepts.AddRange(createUpdateRoleDto.DeptIdArray.Select(d => new RoleDepartment
            { RoleId = role.Id, DeptId = d }));
            await SugarClient.Insertable(roleDepts).ExecuteCommandAsync(ct);
        }

        return OperateResult.Success();
    }

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Roles.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateRoleDto createUpdateRoleDto, CancellationToken ct = default)
    {
        //取出待更新数据
        var oldRole = await TableWhere(x => x.Id == createUpdateRoleDto.Id).Includes(x => x.Users).FirstAsync(ct);
        if (oldRole.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateRoleDto, "角色",
                nameof(createUpdateRoleDto.Id)));
        }

        if (oldRole.Name != createUpdateRoleDto.Name &&
            await TableWhere(x => x.Name == createUpdateRoleDto.Name).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.Name)));
        }

        if (oldRole.AuthCode != createUpdateRoleDto.AuthCode &&
            await TableWhere(x => x.AuthCode == createUpdateRoleDto.AuthCode).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateRoleDto,
                nameof(createUpdateRoleDto.AuthCode)));
        }

        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize && createUpdateRoleDto.DeptIdArray.Count == 0)
        {
            return OperateResult.Error("部门 集合必须至少包含一个项目");
        }


        var role = Mapper.MapTo<Role>(createUpdateRoleDto);
        await UpdateAsync(role, ct: ct);

        //删除部门权限关联
        await SugarClient.Deleteable<RoleDepartment>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync(ct);
        if (createUpdateRoleDto.DataScopeType == DataScopeType.Customize)
        {
            var roleDepts = new List<RoleDepartment>();
            roleDepts.AddRange(createUpdateRoleDto.DeptIdArray.Select(d => new RoleDepartment
            { RoleId = role.Id, DeptId = d }));
            await SugarClient.Insertable(roleDepts).ExecuteCommandAsync(ct);
        }

        await _permissionCache.RoleChangedAsync(role.Id, ct);

        return OperateResult.Success();
    }

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Roles.Delete)]
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var roleIds = await TableWhere(x => ids.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
        if (roleIds.Count == 0)
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        if (await SugarClient.Queryable<UserRole>().AnyAsync(link => roleIds.Contains(link.RoleId), ct))
        {
            return OperateResult.Error(ValidationError.DataAssociationExists());
        }

        //删除角色 角色部门 角色菜单
        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Roles.List)]
    public Task<PagedResultDto<RoleVo>> GetListAsync(RoleQueryCriteria roleQueryCriteria, CancellationToken ct = default)
        => PageAsync<RoleVo>(roleQueryCriteria,
            options => options.IncludePropertyNameList = [nameof(Role.DepartmentList)], ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Roles.Download)]
    public async Task<List<ExportBase>> DownloadAsync(RoleQueryCriteria roleQueryCriteria, CancellationToken ct = default)
    {
        var roles = await TableWhere(roleQueryCriteria.ApplyQueryConditionalModel()).Includes(x => x.DepartmentList)
            .ToListAsync(ct);
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(roles.Select(x => new RoleExport
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description!,
            DataScopeType = x.DataScopeType,
            DataDept = string.Join(",", x.DepartmentList?.Select(d => d.Name) ?? Enumerable.Empty<string>()),
            AuthCode = x.AuthCode,
            CreationTime = x.CreationTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Roles.List, SystemPermissions.Users.List, SystemPermissions.Users.Add, SystemPermissions.Users.Edit)]
    public async Task<List<RoleVo>> GetAllAsync(CancellationToken ct = default)
    {
        var roleList = await Table.Includes(x => x.MenuList).Includes(x => x.DepartmentList).ToListAsync(ct);

        return Mapper.MapTo<List<RoleVo>>(roleList);
    }

    /// <inheritdoc />
    [UseTran]
    [HasRole(RoleCodes.Admin)]
    public async Task<OperateResult> UpdateRoleMenuAsync(UpdateRoleMenuDto updateRoleMenuDto, CancellationToken ct = default)
    {
        var role = await TableWhere(x => x.Id == updateRoleMenuDto.Id).Includes(x => x.Users).FirstAsync(ct);
        if (role.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist());
        }

        //删除菜单
        List<RoleMenu> roleMenus = new List<RoleMenu>();
        roleMenus.AddRange(updateRoleMenuDto.MenuIdArray.Select(m => new RoleMenu
        { RoleId = role.Id, MenuId = m }));

        await SugarClient.Deleteable<RoleMenu>().Where(x => x.RoleId == role.Id).ExecuteCommandAsync(ct);
        await SugarClient.Insertable(roleMenus).ExecuteCommandAsync(ct);


        await _permissionCache.RoleChangedAsync(role.Id, ct);

        return OperateResult.Success();
    }

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<RoleVo> QueryWithDetailsAsync(long id, CancellationToken ct = default)
    {
        var role = await TableWhere(x => x.Id == id)
            .Includes(x => x.MenuList)
            .Includes(x => x.DepartmentList)
            .FirstAsync(ct);
        return Mapper.MapTo<RoleVo>(role);
    }

    #endregion

}
