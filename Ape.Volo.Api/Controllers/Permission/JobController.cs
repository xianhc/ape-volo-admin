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
using Ape.Volo.ViewModel.Core.Permission.Job;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Permission;

/// <summary>
/// 岗位管理
/// </summary>
[Area("Area.JobManagement")]
[Route("/job", Order = 6)]
public class JobController : BaseApiController
{
    #region 字段

    private readonly IJobService _jobService;

    #endregion

    #region 构造函数

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增岗位
    /// </summary>
    /// <param name="createUpdateJobDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateJobDto createUpdateJobDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _jobService.CreateAsync(createUpdateJobDto);
        return Ok(result);
    }

    /// <summary>
    /// 更新岗位
    /// </summary>
    /// <param name="createUpdateJobDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateJobDto createUpdateJobDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _jobService.UpdateAsync(createUpdateJobDto);
        return Ok(result);
    }

    /// <summary>
    /// 删除岗位
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

        var result = await _jobService.DeleteAsync(idCollection.IdArray);
        return Ok(result);
    }

    /// <summary>
    /// 查看岗位列表
    /// </summary>
    /// <param name="jobQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<JobVo>>))]
    public async Task<ActionResult> Query(JobQueryCriteria jobQueryCriteria, Pagination pagination)
    {
        var jobList = await _jobService.QueryAsync(jobQueryCriteria, pagination);

        return JsonContent(jobList, pagination);
    }

    /// <summary>
    /// 获取所有岗位
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("queryAll")]
    [Description("Action.GetAllJobs")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<JobVo>))]
    public async Task<ActionResult> QueryAll()
    {
        var jobList = await _jobService.QueryAllAsync();

        return JsonContent(jobList);
    }

    /// <summary>
    /// 导出岗位
    /// </summary>
    /// <param name="jobQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(JobQueryCriteria jobQueryCriteria)
    {
        var jobExports = await _jobService.DownloadAsync(jobQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(jobExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.Job") + fileName };
    }

    #endregion
}