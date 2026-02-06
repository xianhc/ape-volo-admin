using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Helper;
using Ape.Volo.Core;
using Ape.Volo.IBusiness.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Log;

/// <summary>
/// 作业日志管理
/// </summary>
[Area("Area.TaskLogManagement")]
[Route("/taskLog", Order = 15)]
public class TaskLogController : BaseApiController
{
    #region 字段

    private readonly IQuartzNetLogService _quartzNetLogService;

    #endregion

    #region 构造函数

    public TaskLogController(IQuartzNetLogService quartzNetLogService)
    {
        _quartzNetLogService = quartzNetLogService;
    }

    #endregion

    /// <summary>
    /// 作业调度执行日志
    /// </summary>
    /// <param name="quartzNetLogQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Action.ExecutionLogJob")]
    public async Task<ActionResult> Query(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria,
        Pagination pagination)
    {
        var quartzNetLogList = await _quartzNetLogService.QueryAsync(quartzNetLogQueryCriteria, pagination);

        return JsonContent(quartzNetLogList, pagination);
    }

    /// <summary>
    /// 导出作业日志
    /// </summary>
    /// <param name="quartzNetLogQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [HasRole(["admin"])]
    public async Task<ActionResult> Download(QuartzNetLogQueryCriteria quartzNetLogQueryCriteria)
    {
        var quartzNetLogExports = await _quartzNetLogService.DownloadAsync(quartzNetLogQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(quartzNetLogExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.TaskLog") + fileName };
    }
}