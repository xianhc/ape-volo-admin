using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using Ape.Volo.IBusiness.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.Monitor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Log;

/// <summary>
/// 异常日志管理
/// </summary>
[Area("Area.ExceptionLogManagement")]
[Route("/exceptionLog", Order = 14)]
public class ExceptionLogController : BaseApiController
{
    #region 字段

    private readonly IExceptionLogService _exceptionLogService;

    #endregion

    #region 构造函数

    public ExceptionLogController(IExceptionLogService exceptionLogService)
    {
        _exceptionLogService = exceptionLogService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 查看异常日志列表
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [NotOperate]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<ExceptionLogVo>>))]
    public async Task<ActionResult> Query(LogQueryCriteria logQueryCriteria,
        Pagination pagination)
    {
        var exceptionLogs = await _exceptionLogService.QueryAsync(logQueryCriteria, pagination);

        return JsonContent(exceptionLogs, pagination);
    }

    #endregion
}