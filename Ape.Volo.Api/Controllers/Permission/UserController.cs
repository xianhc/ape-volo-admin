using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.SharedModel.Dto.Core.Permission.User;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.Permission;
using Ape.Volo.ViewModel.Core.Permission.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Permission;

/// <summary>
/// 用户管理
/// </summary>
[Area("Area.UserManagement")]
[Route("/user", Order = 1)]
public class UserController : BaseApiController
{
    #region 字段

    private readonly IUserService _userService;

    #endregion

    #region 构造函数

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Description("Sys.Create")]
    [Route("create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create([FromBody] CreateUpdateUserDto createUpdateUserDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _userService.CreateAsync(createUpdateUserDto);

        return Ok(result);
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="createUpdateUserDto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPut]
    [Description("Sys.Edit")]
    [Route("edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update([FromBody] CreateUpdateUserDto createUpdateUserDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _userService.UpdateAsync(createUpdateUserDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除用户
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

        var result = await _userService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 修改基础信息
    /// </summary>
    /// <param name="updateUserCenterDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("update/center")]
    [Description("Action.UpdatePersonalInfo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdateCenterAsync([FromBody] UpdateUserCenterDto updateUserCenterDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _userService.UpdateCenterAsync(updateUserCenterDto);
        return Ok(result);
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="updateUserPassDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("update/password")]
    [Description("Action.UpdatePassword")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> UpdatePasswordAsync([FromBody] UpdateUserPassDto updateUserPassDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _userService.UpdatePasswordAsync(updateUserPassDto);
        return Ok(result);
    }

    /// <summary>
    /// 修改邮箱
    /// </summary>
    /// <param name="updateUserEmailDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("update/email")]
    [Description("Action.UpdateEmail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> UpdateEmail([FromBody] UpdateUserEmailDto updateUserEmailDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _userService.UpdateEmailAsync(updateUserEmailDto);
        return Ok(result);
    }

    /// <summary>
    /// 修改头像
    /// </summary>
    /// <param name="avatar"></param>
    /// <returns></returns>
    [HttpPost, HttpOptions]
    [Route("update/avatar")]
    [Description("Action.UpdateAvatar")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> UpdateAvatar([FromForm] IFormFile avatar) //多文件使用  IFormFileCollection
    {
        if (avatar.IsNull())
        {
            return Error(App.L.R("{0}required", "avatar"));
        }

        var result = await _userService.UpdateAvatarAsync(avatar);
        return Ok(result);
    }


    /// <summary>
    /// 查看用户列表
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Query")]
    [Route("query")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<UserVo>>))]
    public async Task<ActionResult> Query(UserQueryCriteria userQueryCriteria,
        Pagination pagination)
    {
        var list = await _userService.QueryAsync(userQueryCriteria, pagination);
        return JsonContent(list, pagination);
    }

    /// <summary>
    /// 导出用户列表
    /// </summary>
    /// <param name="userQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(UserQueryCriteria userQueryCriteria)
    {
        var userExports = await _userService.DownloadAsync(userQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(userExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.User") + fileName };
    }

    #endregion

    #region 扩展修改

    /// <summary>
    /// 更新用户角色
    /// </summary>
    /// <param name="updateUserRole"></param>
    /// <returns></returns>
    [HttpPut]
    [Description("Sys.Edit")]
    [Route("editRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdateRole([FromBody] UpdateUserRole updateUserRole)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _userService.UpdateRoleAsync(updateUserRole);
        return Ok(result);
    }


    /// <summary>
    /// 更新用户岗位
    /// </summary>
    /// <param name="updateUserJob"></param>
    /// <returns></returns>
    [HttpPut]
    [Description("Sys.Edit")]
    [Route("editJob")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdateRole([FromBody] UpdateUserJob updateUserJob)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _userService.UpdateRoleAsync(updateUserJob);
        return Ok(result);
    }

    #endregion
}