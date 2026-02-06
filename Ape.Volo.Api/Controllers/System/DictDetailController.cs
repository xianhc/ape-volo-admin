using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System.Dict;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System.Dict;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.System;

/// <summary>
/// 字典详情管理
/// </summary>
[Area("Area.DictionaryDetailManagement")]
[Route("/dictDetail", Order = 8)]
public class DictDetailController : BaseApiController
{
    #region 字段

    private readonly IDictDetailService _dictDetailService;

    #endregion

    #region 构造函数

    public DictDetailController(IDictDetailService dictDetailService)
    {
        _dictDetailService = dictDetailService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增字典详情
    /// </summary>
    /// <param name="createUpdateDictDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateDictDetailDto createUpdateDictDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _dictDetailService.CreateAsync(createUpdateDictDto);
        return Ok(result);
    }


    /// <summary>
    /// 更新字典详情
    /// </summary>
    /// <param name="createUpdateDictDetailDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateDictDetailDto createUpdateDictDetailDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _dictDetailService.UpdateAsync(createUpdateDictDetailDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除字典详情
    /// </summary>
    /// <param name="idCollection"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route("delete")]
    [Description("Sys.Delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Delete([FromBody] IdCollection idCollection)
    {
        var result = await _dictDetailService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 查看字典详情列表
    /// </summary>
    /// <param name="dictDetailQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<DictDetailVo>>))]
    public async Task<ActionResult> Query(DictDetailQueryCriteria dictDetailQueryCriteria, Pagination pagination)
    {
        var list = await _dictDetailService.QueryAsync(dictDetailQueryCriteria, pagination);
        return JsonContent(list, pagination);
    }

    #endregion
}
