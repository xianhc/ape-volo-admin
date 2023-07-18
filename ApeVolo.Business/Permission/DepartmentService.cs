using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApeVolo.Business.Base;
using ApeVolo.Common.AttributeExt;
using ApeVolo.Common.Exception;
using ApeVolo.Common.Extention;
using ApeVolo.Common.Global;
using ApeVolo.Common.Helper;
using ApeVolo.Common.Model;
using ApeVolo.Common.Resources;
using ApeVolo.Common.WebApp;
using ApeVolo.Entity.Permission;
using ApeVolo.IBusiness.Dto.Permission;
using ApeVolo.IBusiness.ExportModel.Permission;
using ApeVolo.IBusiness.Interface.Permission;
using ApeVolo.IBusiness.QueryModel;
using Castle.Core.Internal;
using SqlSugar;

namespace ApeVolo.Business.Permission;

public class DepartmentService : BaseServices<Department>, IDepartmentService
{
    #region 构造函数

    public DepartmentService(ApeContext apeContext) : base(apeContext)
    {
    }

    #endregion

    #region 基础方法

    [UseTran]
    public async Task<bool> CreateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto)
    {
        if (await TableWhere(d => d.Name == createUpdateDepartmentDto.Name).AnyAsync())
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Department"),
                createUpdateDepartmentDto.Name));
        }

        Department dept =
            ApeContext.Mapper.Map<Department>(createUpdateDepartmentDto);
        await AddEntityAsync(dept);

        //重新计算子节点个数
        if (!dept.ParentId.IsNullOrEmpty())
        {
            var department = await SugarRepository.QueryFirstAsync(x => x.Id == dept.ParentId);
            if (department.IsNotNull())
            {
                var departmentList =
                    await SugarRepository.QueryListAsync(x => x.ParentId == department.Id);
                department.SubCount = departmentList.Count;
                await UpdateEntityAsync(department);
            }
        }

        return true;
    }

    [UseTran]
    public async Task<bool> UpdateAsync(CreateUpdateDepartmentDto createUpdateDepartmentDto)
    {
        var oldUseDepartment =
            await TableWhere(x => x.Id == createUpdateDepartmentDto.Id).FirstAsync();
        if (oldUseDepartment.IsNull())
        {
            throw new BadRequestException(Localized.Get("DataNotExist"));
        }

        if (oldUseDepartment.Name != createUpdateDepartmentDto.Name &&
            await TableWhere(x => x.Name == createUpdateDepartmentDto.Name).AnyAsync())
        {
            throw new BadRequestException(Localized.Get("{0}{1}IsExist", Localized.Get("Department"),
                createUpdateDepartmentDto.Name));
        }

        Department dept =
            ApeContext.Mapper.Map<Department>(createUpdateDepartmentDto);
        dept.SubCount = oldUseDepartment.SubCount;
        await UpdateEntityAsync(dept);

        //重新计算子节点个数
        //判断修改前父部门是否与修改后相同  如果相同说明并没有修改上下级部门信息
        if (oldUseDepartment.ParentId != dept.ParentId)
        {
            if (!dept.ParentId.IsNullOrEmpty())
            {
                var department = await SugarRepository.QueryFirstAsync(x => x.Id == dept.ParentId);
                if (department.IsNotNull())
                {
                    var departmentList =
                        await SugarRepository.QueryListAsync(x => x.ParentId == department.Id);
                    department.SubCount = departmentList.Count;
                    await UpdateEntityAsync(department);
                }
            }

            if (!oldUseDepartment.ParentId.IsNullOrEmpty())
            {
                var department =
                    await SugarRepository.QueryFirstAsync(x => x.Id == oldUseDepartment.ParentId);
                if (department.IsNotNull())
                {
                    var departmentList =
                        await SugarRepository.QueryListAsync(x => x.ParentId == department.Id);
                    department.SubCount = departmentList.Count;
                    await UpdateEntityAsync(department);
                }
            }
        }

        return true;
    }

    [UseTran]
    public async Task<bool> DeleteAsync(HashSet<long> ids)
    {
        List<long> idList = new List<long>();
        foreach (var id in ids)
        {
            if (!idList.Contains(id))
            {
                idList.Add(id);
            }

            var departments = await TableWhere(m => m.ParentId == id).ToListAsync();
            await FindChildIds(departments, idList);
        }

        var departmentList = await TableWhere(x => ids.Contains(x.Id)).ToListAsync();
        await LogicDelete<Department>(x => ids.Contains(x.Id));

        HashSet<long> uPIds = new HashSet<long>();

        departmentList.ForEach(d =>
        {
            if (d.ParentId.IsNotNull())
            {
                uPIds.Add(Convert.ToInt64(d.ParentId));
            }
        });

        foreach (var pid in uPIds)
        {
            var department = await SugarRepository.QueryFirstAsync(x => x.Id == pid);
            if (!department.IsNotNull()) continue;

            var depts =
                await SugarRepository.QueryListAsync(x => x.ParentId == department.Id);
            department.SubCount = depts.Count;
            await UpdateEntityAsync(department);
        }

        return true;
    }


    public async Task<List<DepartmentDto>> QueryAsync(DeptQueryCriteria deptQueryCriteria,
        Pagination pagination)
    {
        var whereExpression = GetWhereExpression(deptQueryCriteria);
        var deptList = await SugarRepository.QueryPageListAsync(whereExpression, pagination);
        var deptDatalist = ApeContext.Mapper.Map<List<DepartmentDto>>(deptList);

        pagination.TotalElements = deptDatalist.Count;
        return deptDatalist;
    }

    public async Task<List<ExportBase>> DownloadAsync(DeptQueryCriteria deptQueryCriteria)
    {
        var whereExpression = GetWhereExpression(deptQueryCriteria);
        var depts = await TableWhere(whereExpression).ToListAsync();
        List<ExportBase> roleExports = new List<ExportBase>();
        roleExports.AddRange(depts.Select(x => new DepartmentExport()
        {
            Id = x.Id,
            Name = x.Name,
            ParentId = x.ParentId ?? 0,
            Sort = x.Sort,
            EnabledState = x.Enabled ? EnabledState.Enabled : EnabledState.Disabled,
            SubCount = x.SubCount,
            CreateTime = x.CreateTime
        }));
        return roleExports;
    }

    #endregion

    #region 扩展方法

    public async Task<List<DepartmentDto>> QuerySuperiorDeptAsync(HashSet<long> ids)
    {
        var departmentList = new List<DepartmentDto>();
        foreach (var id in ids)
        {
            var dept = await TableWhere(x => x.Id == id).FirstAsync();
            var deptDto = ApeContext.Mapper.Map<DepartmentDto>(dept);
            var departmentDtoList = await FindSuperiorAsync(deptDto, new List<DepartmentDto>());
            departmentList.AddRange(departmentDtoList);
        }

        departmentList = TreeHelper<DepartmentDto>.ListToTrees(departmentList, "Id", "ParentId", null);

        return departmentList;
    }

    public async Task<List<DepartmentDto>> QueryByPIdAsync(long id)
    {
        return ApeContext.Mapper.Map<List<DepartmentDto>>(await SugarRepository.QueryListAsync(x =>
            x.ParentId == id && x.Enabled));
    }

    public async Task<DepartmentSmallDto> QueryByIdAsync(long id)
    {
        return ApeContext.Mapper.Map<DepartmentSmallDto>(await SugarRepository.QueryFirstAsync(x =>
            x.Id == id && x.Enabled));
    }


    public async Task<List<DepartmentDto>> QueryByRoleIdAsync(long roleId)
    {
        var departments =
            await SugarRepository
                .QueryMuchAsync<Department, RolesDepartments, Department>(
                    (d, rd) => new object[]
                    {
                        JoinType.Left, d.Id == rd.DeptId
                    }, (d, rd) => d,
                    (d, rd) => roleId == rd.RoleId
                );
        departments = TreeHelper<Department>.SetLeafProperty(departments, "Id", "PId", null);
        return ApeContext.Mapper.Map<List<DepartmentDto>>(departments);
    }

    public async Task<List<long>> FindChildIds(List<long> deptIds, List<DepartmentDto> departmentDtos)
    {
        foreach (var dept in departmentDtos)
        {
            if (!dept.Enabled) continue;
            if (!deptIds.Contains(dept.Id))
            {
                deptIds.Add(dept.Id);
            }

            List<DepartmentDto> deptLists = await QueryByPIdAsync(dept.Id);
            if (deptLists != null && deptLists.Count > 0)
            {
                await FindChildIds(deptIds, deptLists);
            }
        }

        return await Task.FromResult(deptIds);
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取顶级部门
    /// </summary>
    /// <returns></returns>
    private async Task<List<DepartmentDto>> FindByPIdIsNullAsync()
    {
        return ApeContext.Mapper.Map<List<DepartmentDto>>(
            await SugarRepository.QueryListAsync(x => x.ParentId == null && x.Enabled));
    }

    /// <summary>
    /// 查找同级和所有上级部门
    /// </summary>
    /// <param name="departmentDto"></param>
    /// <param name="departmentDtoList"></param>
    /// <returns></returns>
    private async Task<List<DepartmentDto>> FindSuperiorAsync(DepartmentDto departmentDto,
        List<DepartmentDto> departmentDtoList)
    {
        while (true)
        {
            if (departmentDto.ParentId.IsNull())
            {
                departmentDtoList.AddRange(await FindByPIdIsNullAsync());
                return departmentDtoList;
            }

            departmentDtoList.AddRange(await QueryByPIdAsync(Convert.ToInt64(departmentDto.ParentId)));
            departmentDto =
                ApeContext.Mapper.Map<DepartmentDto>(await TableWhere(x => x.Id == departmentDto.ParentId)
                    .FirstAsync());
        }
    }

    /// <summary>
    /// 查找所有下级部门
    /// </summary>
    /// <param name="departmentList"></param>
    /// <param name="ids"></param>
    /// <returns></returns>
    private async Task FindChildIds(List<Department> departmentList, List<long> ids)
    {
        if (departmentList is { Count: > 0 })
        {
            foreach (var department in departmentList)
            {
                if (!ids.Contains(department.Id))
                {
                    ids.Add(department.Id);
                }

                List<Department> departments =
                    await SugarRepository.QueryListAsync(m => m.ParentId == department.Id);
                if (departments is { Count: > 0 })
                {
                    await FindChildIds(departments, ids);
                }
            }
        }

        await Task.FromResult(ids);
    }

    #endregion

    #region 条件表达式

    private static Expression<Func<Department, bool>> GetWhereExpression(DeptQueryCriteria deptQueryCriteria)
    {
        Expression<Func<Department, bool>> whereExpression = x => true;
        whereExpression = deptQueryCriteria.ParentId.IsNotNull()
            ? whereExpression.AndAlso(x => x.ParentId == deptQueryCriteria.ParentId)
            : whereExpression.AndAlso(x => x.ParentId == null);
        if (!deptQueryCriteria.DeptName.IsNullOrEmpty())
        {
            whereExpression = whereExpression.AndAlso(x => x.Name.Contains(deptQueryCriteria.DeptName));
        }

        if (!deptQueryCriteria.Enabled.IsNullOrEmpty())
        {
            whereExpression = whereExpression.AndAlso(x => x.Enabled == deptQueryCriteria.Enabled);
        }

        return whereExpression;
    }

    #endregion
}