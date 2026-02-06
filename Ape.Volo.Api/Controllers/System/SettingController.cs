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
/// 系统配置
/// </summary>
[Area("Area.GlobalSettingManagement")]
[Route("/setting", Order = 10)]
public class SettingController : BaseApiController
{
    #region 字段

    private readonly ISettingService _settingService;

    #endregion

    #region 构造函数

    public SettingController(ISettingService settingService)
    {
        _settingService = settingService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增参数
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateSettingDto createUpdateSettingDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _settingService.CreateAsync(createUpdateSettingDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新参数
    /// </summary>
    /// <param name="createUpdateSettingDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateSettingDto createUpdateSettingDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _settingService.UpdateAsync(createUpdateSettingDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除参数
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

        var result = await _settingService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 获取参数列表
    /// </summary>
    /// <param name="parameterQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<SettingVo>>))]
    public async Task<ActionResult> Query(ParameterQueryCriteria parameterQueryCriteria, Pagination pagination)
    {
        var settingList = await _settingService.QueryAsync(parameterQueryCriteria, pagination);

        return JsonContent(settingList, pagination);
    }


    /// <summary>
    /// 导出参数
    /// </summary>
    /// <param name="parameterQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(ParameterQueryCriteria parameterQueryCriteria)
    {
        var settingExports = await _settingService.DownloadAsync(parameterQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(settingExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.Parameter") + fileName };
    }

    #endregion
}