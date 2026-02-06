using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.Department;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Permission;

/// <summary>
/// 部门管理
/// </summary>
[Area("Area.DepartmentManagement")]
[Route("/dept", Order = 4)]
public class DeptController : BaseApiController
{
    #region 构造函数

    public DeptController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    #endregion

    #region 字段

    private readonly IDepartmentService _departmentService;

    #endregion

    #region 对内接口

    /// <summary>
    /// 新增部门
    /// </summary>
    /// <param name="createUpdateDepartmentDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateDepartmentDto createUpdateDepartmentDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _departmentService.CreateAsync(createUpdateDepartmentDto);
        return Ok(result);
    }


    /// <summary>
    /// 更新部门
    /// </summary>
    /// <param name="createUpdateDepartmentDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateDepartmentDto createUpdateDepartmentDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _departmentService.UpdateAsync(createUpdateDepartmentDto);
        return Ok(result);
    }


    /// <summary>
    /// 删除部门
    /// </summary>
    /// <param name="idCollection"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("delete")]
    [Description("Sys.Delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Delete([FromBody] IdCollection idCollection)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        List<long> ids = new List<long>(idCollection.IdArray);
        var result = await _departmentService.DeleteAsync(ids);
        return Ok(result);
    }

    /// <summary>
    /// 查看部门列表
    /// </summary>
    /// <param name="deptQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<DepartmentVo>>))]
    public async Task<ActionResult> Query(DeptQueryCriteria deptQueryCriteria,
        Pagination pagination)
    {
        var deptList = await _departmentService.QueryAsync(deptQueryCriteria, pagination);


        return JsonContent(deptList, pagination);
    }


    [HttpGet]
    [Route("queryTree")]
    [Description("Action.GetDepartmentTreeData")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DepartmentVo>))]
    public async Task<ActionResult> QueryTree()
    {
        var deptList = await _departmentService.QueryAllAsync();

        var departmentVos = TreeHelper<DepartmentVo>.ListToTrees(deptList, "Id", "ParentId", 0);
        return JsonContent(departmentVos);
    }


    /// <summary>
    /// 导出部门
    /// </summary>
    /// <param name="deptQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(DeptQueryCriteria deptQueryCriteria)
    {
        var deptExports = await _departmentService.DownloadAsync(deptQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(deptExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.Department") + fileName };
    }


    /// <summary>
    /// 获取同级与父级部门
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("superior")]
    [Description("Action.GetSiblingAndParentDepartments")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DepartmentVo>))]
    public async Task<ActionResult> GetSuperior(long id)
    {
        var deptList = await _departmentService.QuerySuperiorDeptAsync(id);

        return JsonContent(deptList);
    }

    #endregion
}