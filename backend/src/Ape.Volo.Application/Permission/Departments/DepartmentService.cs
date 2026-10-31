using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Permission.Departments.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Permission;
using Ape.Volo.Entity.Permission.Roles;
using Ape.Volo.Entity.Permission.Users;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Permission.Departments;

/// <summary>
/// 部门服务
/// </summary>
public class DepartmentService : BaseService<Department>, IDepartmentService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public DepartmentService(ISugarRepository<Department> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
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
            return OperateResult.Error(ValidationError.IsExist(createUpdateDepartmentDto,
                nameof(createUpdateDepartmentDto.Name)));
        }

        Department dept =
            Mapper.MapTo<Department>(createUpdateDepartmentDto);
        await AddAsync(dept, ct: ct);

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
            return OperateResult.Error(ValidationError.NotExist(createUpdateDepartmentDto,
                "部门",
                nameof(createUpdateDepartmentDto.Id)));
        }

        if (oldUseDepartment.Name != createUpdateDepartmentDto.Name &&
            await TableWhere(x => x.Name == createUpdateDepartmentDto.Name).AnyAsync(ct))
        {
            return OperateResult.Error(ValidationError.IsExist(createUpdateDepartmentDto,
                nameof(createUpdateDepartmentDto.Name)));
        }

        Department dept =
            Mapper.MapTo<Department>(createUpdateDepartmentDto);
        await UpdateAsync(dept, ct: ct);

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
            return OperateResult.Error(ValidationError.NotExist());
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

    #region 扩展方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Departments.List)]
    public async Task<List<DepartmentVo>> QuerySuperiorDeptAsync(long id, CancellationToken ct = default)
    {
        var departmentList = new List<DepartmentVo>();
        var dept = await TableWhere(x => x.Id == id).FirstAsync(ct);
        var deptDto = Mapper.MapTo<DepartmentVo>(dept);
        var departmentVoList = await FindSuperiorAsync(deptDto, new List<DepartmentVo>(), ct);
        departmentList.AddRange(departmentVoList);

        departmentList = TreeHelper<DepartmentVo>.ListToTrees(departmentList, "Id", "ParentId", null);

        return departmentList;
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Departments.List)]
    public async Task<List<DepartmentVo>> QueryByPIdAsync(long id, CancellationToken ct = default)
    {
        return Mapper.MapTo<List<DepartmentVo>>(await TableWhere(x =>
            x.ParentId == id && x.Enabled).ToListAsync(ct));
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Departments.List)]
    public async Task<DepartmentSmallVo> QueryByIdAsync(long id, CancellationToken ct = default)
    {
        return Mapper.MapTo<DepartmentSmallVo>(await TableWhere(x =>
            x.Id == id && x.Enabled).FirstAsync(ct));
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取顶级部门
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    private async Task<List<DepartmentVo>> FindByPIdIsNullAsync(CancellationToken ct = default)
    {
        return Mapper.MapTo<List<DepartmentVo>>(
            await TableWhere(x => x.ParentId == null && x.Enabled).ToListAsync(ct));
    }

    /// <summary>
    /// 查找同级和所有上级部门
    /// </summary>
    /// <param name="departmentVo"></param>
    /// <param name="departmentVoList"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    private async Task<List<DepartmentVo>> FindSuperiorAsync(DepartmentVo departmentVo,
        List<DepartmentVo> departmentVoList, CancellationToken ct = default)
    {
        while (true)
        {
            if (departmentVo.ParentId is null)
            {
                departmentVoList.AddRange(await FindByPIdIsNullAsync(ct));
                return departmentVoList;
            }

            departmentVoList.AddRange(await QueryByPIdAsync(departmentVo.ParentId.Value, ct));
            var dto = departmentVo;
            departmentVo =
                Mapper.MapTo<DepartmentVo>(await TableWhere(x => x.Id == dto.ParentId!.Value)
                    .FirstAsync(ct));
        }
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Departments.List)]
    public async Task<List<long>> GetChildIds(List<long> ids, List<long>? allIds, CancellationToken ct = default)
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
