using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.TaskService.service;
using Ape.Volo.ViewModel.Core.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace Ape.Volo.Api.Controllers.System;

/// <summary>
/// 作业管理
/// </summary>
[Area("Area.JobSchedulingManagement")]
[Route("/timing", Order = 9)]
public class QuartzNetController : BaseApiController
{
    #region 字段

    private readonly IQuartzNetService _quartzNetService;
    private readonly ISchedulerCenterService _schedulerCenterService;

    #endregion

    #region 构造函数

    public QuartzNetController(IQuartzNetService quartzNetService, ISchedulerCenterService schedulerCenterService)
    {
        _quartzNetService = quartzNetService;
        _schedulerCenterService = schedulerCenterService;
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 新增作业
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("create")]
    [Description("Sys.Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Create(
        [FromBody] CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        if (createUpdateQuartzNetDto.TriggerType == TriggerType.Cron)
        {
            if (createUpdateQuartzNetDto.Cron.IsNullOrEmpty())
            {
                return Error(ValidationError.Required(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.Cron)));
            }

            if (!CronExpression.IsValidExpression(createUpdateQuartzNetDto.Cron))
            {
                return Error(App.L.R("{0}Error.Format", "cron"));
            }
        }
        else if (createUpdateQuartzNetDto.TriggerType == TriggerType.Simple)
        {
            if (createUpdateQuartzNetDto.IntervalSecond <= 5)
            {
                return Error(App.L.R("Error.SetIntervalSeconds"));
            }
        }

        var quartzNet = await _quartzNetService.CreateAsync(createUpdateQuartzNetDto);
        if (quartzNet.IsNotNull())
        {
            if (quartzNet.Enabled)
            {
                //开启作业任务
                await _schedulerCenterService.AddScheduleJobAsync(quartzNet);
            }
        }

        return Ok(OperateResult.Result(quartzNet.IsNotNull()));
    }

    /// <summary>
    /// 更新作业
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    [HttpPut]
    [Route("edit")]
    [Description("Sys.Edit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(
        [FromBody] CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        if (createUpdateQuartzNetDto.TriggerType == TriggerType.Cron)
        {
            if (createUpdateQuartzNetDto.Cron.IsNullOrEmpty())
            {
                return Error(ValidationError.Required(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.Cron)));
            }

            if (!CronExpression.IsValidExpression(createUpdateQuartzNetDto.Cron))
            {
                return Error(App.L.R("{0}Error.Format", "cron"));
            }
        }
        else if (createUpdateQuartzNetDto.TriggerType == TriggerType.Simple)
        {
            if (createUpdateQuartzNetDto.IntervalSecond < 1)
            {
                return Error(App.L.R("Error.SetIntervalSeconds"));
            }
        }

        var result = await _quartzNetService.UpdateAsync(createUpdateQuartzNetDto);
        if (result.IsSuccess)
        {
            var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
            await _schedulerCenterService.DeleteScheduleJobAsync(quartzNet.TaskName, quartzNet.TaskGroup);

            if (quartzNet.Enabled)
            {
                await _schedulerCenterService.AddScheduleJobAsync(quartzNet);
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// 删除作业
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

        var quartzList = await _quartzNetService.TableWhere(x => idCollection.IdArray.Contains(x.Id)).ToListAsync();
        if (quartzList.Count > 0)
        {
            var result = await _quartzNetService.DeleteAsync(quartzList);
            if (result.IsSuccess)
            {
                foreach (var item in quartzList)
                {
                    await _schedulerCenterService.DeleteScheduleJobAsync(item.TaskName, item.TaskGroup);
                }
            }

            return Ok(result);
        }

        return Error(ValidationError.NotExist());
    }

    /// <summary>
    /// 获取作业列表
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("query")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<QuartzNetVo>>))]
    public async Task<ActionResult> Query(QuartzNetQueryCriteria quartzNetQueryCriteria,
        Pagination pagination)
    {
        var quartzNetList = await _quartzNetService.QueryAsync(quartzNetQueryCriteria, pagination);

        foreach (var quartzNet in quartzNetList)
        {
            quartzNet.TriggerState =
                await _schedulerCenterService.GetTriggerStatus(quartzNet.TaskName, quartzNet.TaskGroup);
        }

        return JsonContent(quartzNetList, pagination);
    }


    /// <summary>
    /// 获取所有作业
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("queryAll")]
    [Description("Sys.Query")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm<List<QuartzNetSmallVo>>))]
    public async Task<ActionResult> QueryAll()
    {
        var list = await _quartzNetService.QueryAllTaskNameAsync();

        return JsonContent(list);
    }


    /// <summary>
    /// 导出作业调度
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <returns></returns>
    [HttpGet]
    [Description("Sys.Export")]
    [Route("download")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> Download(QuartzNetQueryCriteria quartzNetQueryCriteria)
    {
        var quartzNetExports = await _quartzNetService.DownloadAsync(quartzNetQueryCriteria);
        var data = new ExcelHelper().GenerateExcel(quartzNetExports, out var mimeType, out var fileName);
        return new FileContentResult(data, mimeType) { FileDownloadName = App.L.R("Sys.QuartzNet") + fileName };
    }


    /// <summary>
    /// 执行作业
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut]
    [Description("Action.ExecuteJob")]
    [Route("execute")]
    public async Task<ActionResult> Execute(long id)
    {
        var quartzNet = await _quartzNetService.TableWhere(x => x.Id == id).FirstAsync();
        if (quartzNet.IsNull())
        {
            return Error(ValidationError.NotExist());
        }

        //开启作业任务
        quartzNet.Enabled = true;
        if (await _quartzNetService.UpdateAsync(quartzNet))
        {
            //检查任务在内存状态
            var isTrue = await _schedulerCenterService.IsExistScheduleJobAsync(quartzNet.TaskName, quartzNet.TaskGroup);
            if (!isTrue)
            {
                if (await _schedulerCenterService.AddScheduleJobAsync(quartzNet))
                {
                    return Ok(OperateResult.Success());
                }

                return Ok(OperateResult.Error(App.L.R("Error.ExecutionFailed")));
            }

            return Ok(OperateResult.Error(App.L.R("Error.AlreadyRunning")));
        }

        return Error();
    }

    /// <summary>
    /// 暂停作业
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut]
    [Description("Action.PauseJob")]
    [Route("pause")]
    public async Task<ActionResult> Pause(long id)
    {
        var quartzNet = await _quartzNetService.TableWhere(x => x.Id == id).FirstAsync();
        if (quartzNet.IsNull())
        {
            return Error(ValidationError.NotExist());
        }

        var triggerState = await _schedulerCenterService.GetTriggerStatus(quartzNet.TaskName, quartzNet.TaskGroup);
        if (triggerState == TriggerState.Normal)
        {
            //检查任务在内存状态
            var isTrue = await _schedulerCenterService.IsExistScheduleJobAsync(quartzNet.TaskName, quartzNet.TaskGroup);
            if (isTrue && await _schedulerCenterService.PauseJob(quartzNet.TaskName, quartzNet.TaskGroup))
            {
                return Ok(OperateResult.Success());
            }
        }

        return Error(App.L.R("Error.PauseFailed"));
    }

    /// <summary>
    /// 恢复作业
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut]
    [Description("Action.ResumeJob")]
    [Route("resume")]
    public async Task<ActionResult> Resume(long id)
    {
        var quartzNet = await _quartzNetService.TableWhere(x => x.Id == id).FirstAsync();
        if (quartzNet.IsNull())
        {
            return Error(ValidationError.NotExist());
        }

        var triggerState = await _schedulerCenterService.GetTriggerStatus(quartzNet.TaskName, quartzNet.TaskGroup);
        if (triggerState == TriggerState.Paused)
        {
            //检查任务在内存状态
            var isTrue = await _schedulerCenterService.IsExistScheduleJobAsync(quartzNet.TaskName, quartzNet.TaskGroup);
            if (isTrue && await _schedulerCenterService.ResumeJob(quartzNet.TaskName, quartzNet.TaskGroup))
            {
                return Ok(OperateResult.Success());
            }
        }

        return Error(App.L.R("Error.RestoreFailed"));
    }

    #endregion
}