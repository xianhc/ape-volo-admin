using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.System;

/// <summary>
/// 应用密钥管理
/// </summary>
[Area("Area.ApplicationKeyManagement")]
[Route("/appSecret", Order = 11)]
public class AppSecretController : BaseApiController
{
    #region 字段

    private readonly IAppSecretService _appSecretService;

    #endregion

    #region 构造函数

    public AppSecretController(IAppSecretService appSecretService)
    {
        _appSecretService = appSecretService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增应用密钥
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateAppSecretDto createUpdateAppSecretDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _appSecretService.CreateAsync(createUpdateAppSecretDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新应用密钥
    /// </summary>
    /// <param name="createUpdateAppSecretDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateAppSecretDto createUpdateAppSecretDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _appSecretService.UpdateAsync(createUpdateAppSecretDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除应用密钥
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

        var result = await _appSecretService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 获取应用密钥列表
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<AppSecretVo>>))]
    public async Task<ActionResult> Query(AppsecretQueryCriteria appsecretQueryCriteria,
        Pagination pagination)
    {
        var appSecretList = await _appSecretService.QueryAsync(appsecretQueryCriteria, pagination);

        return JsonContent(appSecretList, pagination);
    }


    /// <summary>
    /// 导出应用密钥
    /// </summary>
    /// <param name="appsecretQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(AppsecretQueryCriteria appsecretQueryCriteria)
    {
        var appSecretExports = await _appSecretService.DownloadAsync(appsecretQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(appSecretExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.AppSecret") + fileName };
    }

    #endregion
}