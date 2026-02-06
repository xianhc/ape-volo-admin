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
using Ape.Volo.ViewModel.Core.Permission.Menu;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Permission;

/// <summary>
/// 菜单管理
/// </summary>
[Area("Area.MenuManagement")]
[Route("/menu", Order = 4)]
public class MenusController : BaseApiController
{
    #region 字段

    private readonly IMenuService _menuService;

    #endregion

    #region 构造函数

    public MenusController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增菜单
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateMenuDto createUpdateMenuDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _menuService.CreateAsync(createUpdateMenuDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新菜单
    /// </summary>
    /// <param name="createUpdateMenuDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateMenuDto createUpdateMenuDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _menuService.UpdateAsync(createUpdateMenuDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除菜单
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

        var result = await _menuService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 构建树形菜单
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Description("Action.BuildLoginMenu")]
    [Route("build")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MenuVo>))]
    public async Task<ActionResult> Build()
    {
        var menuVos = await _menuService.BuildTreeAsync(App.HttpUser.Id);
        return JsonContentIgnoreNullValue(menuVos);
    }

    /// <summary>
    /// 获取子菜单
    /// </summary>
    /// <param name="pid">父级ID</param>
    /// <returns></returns>
    [HttpGet]
    [Description("Action.GetSubMenu")]
    [Route("lazy")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MenuVo>))]
    public async Task<ActionResult<object>> GetMenuLazy(long pid)
    {
        if (pid.IsNullOrEmpty())
        {
            return Error("pid cannot be empty");
        }

        var menuList = await _menuService.FindByPIdAsync(pid);
        return JsonContentIgnoreNullValue(menuList);
    }

    /// <summary>
    /// 查看菜单列表
    /// </summary>
    /// <param name="menuQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Query")]
    [Route("query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<MenuVo>>))]
    public async Task<ActionResult> Query(MenuQueryCriteria menuQueryCriteria)
    {
        var menuList = await _menuService.QueryAsync(menuQueryCriteria);
        return JsonContent(menuList, new Pagination { TotalElements = menuList.Count });
    }


    /// <summary>
    /// 导出菜单
    /// </summary>
    /// <param name="menuQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(MenuQueryCriteria menuQueryCriteria)
    {
        var menuExports = await _menuService.DownloadAsync(menuQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(menuExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.Menu") + fileName };
    }

    /// <summary>
    /// 获取同级与上级菜单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Action.GetSiblingAndParentMenus")]
    [Route("superior")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MenuVo>))]
    public async Task<ActionResult<object>> GetSuperior(long id)
    {
        if (id.IsNullOrEmpty())
        {
            return Error("id cannot be empty");
        }

        var menuVos = await _menuService.FindSuperiorAsync(id);
        return JsonContentIgnoreNullValue(menuVos);
    }

    [HttpGet]
    [Description("Action.GetAllSubMenu")]
    [Route("child")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<long>))]
    public async Task<ActionResult> GetChild(long id)
    {
        var menuIds = await _menuService.FindChildAsync(id);
        return Ok(menuIds);
    }


    /// <summary>
    /// 查询所有菜单
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("queryAll")]
    [Description("Action.GetAllMenu")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MenuVo>))]
    public async Task<ActionResult> QueryAllMenu()
    {
        var menus = await _menuService.QueryAllAsync();

        var menuTree = TreeHelper<MenuVo>.ListToTrees(menus, "Id", "ParentId", 0);
        return JsonContent(menuTree);
    }

    #endregion
}