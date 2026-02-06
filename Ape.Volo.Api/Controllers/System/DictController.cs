using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.ActionExtension.Parameter;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Entity.Test;
using Ape.Volo.IBusiness.System;
using Ape.Volo.IBusiness.Test;
using Ape.Volo.SharedModel.Dto.Core.System.Dict;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System.Dict;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.System;

/// <summary>
/// 字典管理
/// </summary>
[Area("Area.DictionaryManagement")]
[Route("/dict", Order = 7)]
public class DictController : BaseApiController
{
    #region 字段

    private readonly IDictService _dictService;
    private readonly ITestOrderService _testOrderService;

    #endregion

    #region 构造函数

    public DictController(IDictService dictService, ITestOrderService testOrderService)
    {
        _testOrderService = testOrderService;
        _dictService = dictService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增字典
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create([FromBody] CreateUpdateDictDto createUpdateDictDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _dictService.CreateAsync(createUpdateDictDto);
        return Ok(result);
    }


    /// <summary>
    /// 更新字典
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update([FromBody] CreateUpdateDictDto createUpdateDictDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _dictService.UpdateAsync(createUpdateDictDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除字典
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

        var result = await _dictService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 查看字典列表
    /// </summary>
    /// <param name="dictQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<DictVo>>))]
    public async Task<ActionResult> Query(DictQueryCriteria dictQueryCriteria,
        Pagination pagination)
    {
        await _testOrderService.AddAsync(new TestOrder
        {
            Id = IdHelper.NextId(),
            OrderNo = "1001",
            GoodsName = "iphone 16",
            Qty = 1,
            Price = 5000
        });
        var list2 = await _testOrderService.Table.ToListAsync();

        var list = await _dictService.QueryAsync(dictQueryCriteria, pagination);

        return JsonContent(list, pagination);
    }

    /// <summary>
    /// 导出字典
    /// </summary>
    /// <param name="dictQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(DictQueryCriteria dictQueryCriteria)
    {
        var dictExports = await _dictService.DownloadAsync(dictQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(dictExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.Dict") + fileName };
    }


    /// <summary>
    /// 查看字典列表
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("single")]
    [Description("Sys.Query")]
    [ApeVoloOnline]
    [ParamRequired("name")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<DictVo>))]
    public async Task<ActionResult> QueryByName(string name)
    {
        var dictVo = await _dictService.QueryByNameAsync(name);

        return JsonContent(dictVo);
    }

    #endregion
}