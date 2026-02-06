using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Model;
using Ape.Volo.IBusiness.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.LogInfo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Log;

/// <summary>
/// 审计日志管理
/// </summary>
[Area("Area.OperateLogManagement")]
[Route("/operateLog", Order = 13)]
public class OperateLogController : BaseApiController
{
    #region 字段

    private readonly IOperateLogService _operateLogService;
    private readonly IExceptionLogService _exceptionLogService;

    #endregion

    #region 构造函数

    public OperateLogController(IOperateLogService operateLogService, IExceptionLogService exceptionLogService)
    {
        _operateLogService = operateLogService;
        _exceptionLogService = exceptionLogService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 审计列表
    /// </summary>
    /// <param name="logQueryCriteria">查询对象</param>
    /// <param name="pagination">分页对象</param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [NotOperate]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<OperateLogVo>>))]
    public async Task<ActionResult> Query(LogQueryCriteria logQueryCriteria,
        Pagination pagination)
    {
        var operateLogVos = await _operateLogService.QueryAsync(logQueryCriteria, pagination);

        return JsonContent(operateLogVos, pagination);
    }


    /// <summary>
    /// 当前用户行为
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("current")]
    [Description("Action.UserConduct")]
    [NotOperate]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<OperateLogVo>>))]
    public async Task<ActionResult> QueryCurrent(Pagination pagination)
    {
        var operateLogVos = await _operateLogService.QueryByCurrentAsync(pagination);

        return JsonContent(operateLogVos, pagination);
    }

    /// <summary>
    /// 当前用户行为
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("visitTrend")]
    [NotOperate]
    [ApeVoloOnline]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<VisitTrendVo>>))]
    public async Task<ActionResult> QueryVisitTrend(int days = 30)
    {
        if (days < 1 || days > 30)
        {
            return Error("Days must be between 1 and 30.");
        }

        DateTime startDate = DateTime.Now.AddDays(-(days - 1));


        var dateList = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i).ToString("yyyy-MM-dd"))
            .ToList();

        var operateNumbers = await _operateLogService.GetOperationNumber(days);
        var exceptionNumbers = await _exceptionLogService.GetOperationNumber(days);
        VisitTrendVo visitTrendVo = new VisitTrendVo
        {
            DateList = dateList, OperateList = operateNumbers, ExceptionList = exceptionNumbers
        };
        return JsonContent(visitTrendVo);
    }

    #endregion
}