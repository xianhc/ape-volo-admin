using Ape.Volo.Entity.Organization.Departments;
using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Organization.Departments.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Identity;
using Ape.Volo.Entity.Identity.Roles;
using Ape.Volo.Entity.Identity.Users;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Organization.Departments;

/// <summary>
/// 部门服务
/// </summary>
public class DepartmentService : BaseService<Department>, IDepartmentService
{
    private readonly IPermissionCacheService _permissionCache;
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="permissionCache">组织数据范围缓存</param>
    public DepartmentService(ISugarRepository<Department> sugarRepository, IMapper mapper, IPermissionCacheService permissionCache) : base(sugarRepository, mapper)
    {
        _permissionCache = permissionCache;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Departments.Add)]
    public async Task<OperateResult> CreateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto, CancellationToken ct = default)
    {
        if (await TableWhere(d => d.Name == createUpdateDepartmentDto.Name).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateDepartmentDto.Name);
        }

        Department dept =
            Mapper.MapTo<Department>(createUpdateDepartmentDto);
        await AddAsync(dept, ct: ct);
        await _permissionCache.DataScopeChangedAsync(CancellationToken.None);

        return OperateResult.Success();
    }

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Departments.Edit)]
    public async Task<OperateResult> UpdateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto, CancellationToken ct = default)
    {
        var oldUseDepartment =
            await TableWhere(x => x.Id == createUpdateDepartmentDto.Id).FirstAsync(ct);
        if (oldUseDepartment.IsNull())
        {
            return NotFound(createUpdateDepartmentDto.Id);
        }

        if (oldUseDepartment.Name != createUpdateDepartmentDto.Name &&
            await TableWhere(x => x.Name == createUpdateDepartmentDto.Name).AnyAsync(ct))
        {
            return AlreadyExists(() => createUpdateDepartmentDto.Name);
        }

        Department dept =
            Mapper.MapTo<Department>(createUpdateDepartmentDto);
        await UpdateAsync(dept, ct: ct);
        await _permissionCache.DataScopeChangedAsync(CancellationToken.None);

        return OperateResult.Success();
    }

    /// <inheritdoc />
    [UseTran]
    [HasPerm(SystemPermissions.Departments.Delete)]
    public async Task<OperateResult> DeleteAsync(List<long> ids, CancellationToken ct = default)
    {
        var allIds = await GetChildIds(ids, null, ct);
        var departmentIds = await TableWhere(x => allIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(ct);
        if (departmentIds.Count < 1)
        {
            return NotFound();
        }

        if (await SugarClient.Queryable<User>().AnyAsync(user => departmentIds.Contains(user.DeptId), ct))
        {
            return OperateResult.Error(ValidationError.DataAssociationExists());
        }

        if (await SugarClient.Queryable<RoleDepartment>()
                .AnyAsync(link => departmentIds.Contains(link.DeptId), ct))
        {
            return OperateResult.Error(ValidationError.DataAssociationExists());
        }

        var isTrue = await LogicDeleteAsync(x => allIds.Contains(x.Id), ct);
        if (isTrue) await _permissionCache.DataScopeChangedAsync(CancellationToken.None);

        return OperateResult.Result(isTrue);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Departments.List)]
    public async Task<PagedResultDto<DepartmentVo>> GetListAsync(DepartmentQueryCriteria deptQueryCriteria, CancellationToken ct = default)
    {
        // 展开子节点时不分页，直接返回整棵子树
        if (deptQueryCriteria.ParentId.HasValue)
        {
            var children = await TableWhere(deptQueryCriteria.ApplyQueryConditionalModel()).ToListAsync(ct);
            var childVos = Mapper.MapTo<List<DepartmentVo>>(children);
            await FillHasChildrenAsync(childVos, ct);
            return new PagedResultDto<DepartmentVo>(childVos.Count, childVos);
        }

        // 只查顶级部门由 IsRoot 映射成 parent_id IS NULL，随通用条件一起生效
        var page = await PageAsync<DepartmentVo>(deptQueryCriteria, ct: ct);
        await FillHasChildrenAsync(page.Items.ToList(), ct);
        return page;
    }

    /// <summary>
    /// 按当前结果集的主键批量判定哪些行还有子节点。
    /// </summary>
    /// <param name="departmentVos">当前结果集的部门数据</param>
    /// <param name="ct">取消令牌</param>
    private async Task FillHasChildrenAsync(List<DepartmentVo> departmentVos, CancellationToken ct)
    {
        if (departmentVos.Count == 0)
        {
            return;
        }

        var ids = departmentVos.Select(x => x.Id).ToList();
        var parentIds = await TableWhere(x => x.ParentId.HasValue && ids.Contains(x.ParentId.Value))
            .Select(x => x.ParentId).ToListAsync(ct);
        var parentIdSet = parentIds.Where(x => x.HasValue).Select(x => x!.Value).ToHashSet();
        foreach (var departmentVo in departmentVos)
        {
            departmentVo.HasChildren = parentIdSet.Contains(departmentVo.Id);
        }
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Departments.List, SystemPermissions.Users.List, SystemPermissions.Users.Add, SystemPermissions.Users.Edit, SystemPermissions.Roles.List, SystemPermissions.Roles.Add, SystemPermissions.Roles.Edit)]
    public async Task<List<DepartmentVo>> GetAllAsync(CancellationToken ct = default)
    {
        var deptList = Mapper.MapTo<List<DepartmentVo>>(await Table.ToListAsync(ct));
        return deptList;
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Departments.Download)]
    public async Task<List<ExportBase>> DownloadAsync(DepartmentQueryCriteria deptQueryCriteria, CancellationToken ct = default)
    {
        var depts = await TableWhere(deptQueryCriteria.ApplyQueryConditionalModel()).ToListAsync(ct);
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(Mapper.MapTo<List<DepartmentExport>>(depts));
        return roleExports;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取所选部门及全部下级部门ID
    /// </summary>
    /// <param name="ids">起始部门ID集合</param>
    /// <param name="allIds">累计结果，首次调用传 null</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>所选部门及全部下级部门ID</returns>
    private async Task<List<long>> GetChildIds(List<long> ids, List<long>? allIds, CancellationToken ct = default)
    {
        allIds ??= new List<long>();

        foreach (var id in ids.Where(id => !allIds.Contains(id)))
        {
            allIds.Add(id);
            var list = await TableWhere(x => x.ParentId == id && x.Enabled).ToListAsync(ct);
            if (list.Any())
            {
                await GetChildIds(list.Select(x => x.Id).ToList(), allIds, ct);
            }
        }

        return allIds;
    }

    #endregion

}
