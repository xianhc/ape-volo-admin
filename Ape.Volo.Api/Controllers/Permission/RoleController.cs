using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission.Role;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.Role;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Permission;

/// <summary>
/// 角色管理
/// </summary>
[Area("Area.RoleManagement")]
[Route("/role", Order = 2)]
public class RoleController : BaseApiController
{
    #region 字段

    private readonly IRoleService _roleService;

    #endregion

    #region 构造函数

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleService"></param>
    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 添加角色
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create([FromBody] CreateUpdateRoleDto createUpdateRoleDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _roleService.CreateAsync(createUpdateRoleDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="createUpdateRoleDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Description("Sys.Edit")]
    [Route("edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update([FromBody] CreateUpdateRoleDto createUpdateRoleDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _roleService.UpdateAsync(createUpdateRoleDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="idCollection"></param>
    /// <returns></returns>
    [HttpDelete]
    [Description("Sys.Delete")]
    [Route("delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Delete([FromBody] IdCollection idCollection)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _roleService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 查看单一角色
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("single")]
    [Description("Action.ViewSingleRole")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleVo))]
    public async Task<ActionResult> QuerySingle(long id)
    {
        var role = await _roleService.TableWhere(x => x.Id == id).Includes(x => x.MenuList).Includes(x => x.ApiList)
            .Includes(x => x.DepartmentList).SingleAsync();
        return JsonContent(App.Mapper.MapTo<RoleVo>(role));
    }

    /// <summary>
    /// 查看角色列表
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<RoleVo>>))]
    public async Task<ActionResult> Query(RoleQueryCriteria roleQueryCriteria,
        Pagination pagination)
    {
        var roleList = await _roleService.QueryAsync(roleQueryCriteria, pagination);

        return JsonContent(roleList, pagination);
    }

    /// <summary>
    /// 导出角色列表
    /// </summary>
    /// <param name="roleQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(RoleQueryCriteria roleQueryCriteria)
    {
        var roleExports = await _roleService.DownloadAsync(roleQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(roleExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.Role") + fileName };
    }

    /// <summary>
    /// 获取全部角色
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("queryAll")]
    [Description("Action.GetAllRole")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<RoleVo>))]
    public async Task<ActionResult> QueryAll()
    {
        var allRoles = await _roleService.QueryAllAsync();

        return JsonContent(allRoles);
    }


    /// <summary>
    /// 更新角色菜单关联
    /// </summary>
    /// <param name="updateRoleMenuDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("editMenu")]
    [Description("Action.UpdateRoleMenu")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdateRoleMenu([FromBody] UpdateRoleMenuDto updateRoleMenuDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _roleService.UpdateRoleMenuAsync(updateRoleMenuDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新角色Api关联
    /// </summary>
    /// <param name="updateRoleApiDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("editApi")]
    [Description("Action.UpdateRoleApi")]
    public async Task<ActionResult> UpdateRoleApi([FromBody] UpdateRoleApiDto updateRoleApiDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _roleService.UpdateRoleApiAsync(updateRoleApiDto);
        return Ok(result);
    }


    /// <summary>
    /// 获取当前用户角色等级
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("level")]
    [Description("Action.GetRoleLevel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RoleLevelVo))]
    public async Task<ActionResult> GetRoleLevel(int? level)
    {
        var curLevel = await _roleService.VerificationUserRoleLevelAsync(level);

        var response = new RoleLevelVo { Level = curLevel };

        return JsonContent(response);
    }

    #endregion
}