using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Exception;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Core.System;
using Ape.Volo.IBusiness.System;
using Ape.Volo.SharedModel.Dto.Core.System;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.System;
using Ape.Volo.ViewModel.Report.System;

namespace Ape.Volo.Business.System;

/// <summary>
/// QuartzNet作业服务
/// </summary>
public class QuartzNetService : BaseServices<QuartzNet>, IQuartzNetService
{

    #region 基础方法

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <returns></returns>
    public async Task<List<QuartzNet>> QueryAllAsync()
    {
        return await Table.ToListAsync();
    }

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<QuartzNet> CreateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        if (await TableWhere(q => q.TaskName == createUpdateQuartzNetDto.TaskName).AnyAsync())
        {
            throw new BadRequestException(
                ValidationError.IsExist(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.TaskName)));
        }

        if (await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            throw new BadRequestException(
                ValidationError.IsExist(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.ClassName)));
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        return await SugarClient.Insertable(quartzNet).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateQuartzNetDto"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateAsync(CreateUpdateQuartzNetDto createUpdateQuartzNetDto)
    {
        var oldQuartzNet =
            await TableWhere(x => x.Id == createUpdateQuartzNetDto.Id).FirstAsync();
        if (oldQuartzNet.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateQuartzNetDto,
                LanguageKeyConstants.QuartzNet,
                nameof(createUpdateQuartzNetDto.Id)));
        }

        if (oldQuartzNet.TaskName != createUpdateQuartzNetDto.TaskName
            && await TableWhere(q =>
                q.TaskName == createUpdateQuartzNetDto.TaskName).AnyAsync())
        {
            return OperateResult.Error(
                ValidationError.IsExist(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.TaskName)));
        }


        if ((oldQuartzNet.AssemblyName != createUpdateQuartzNetDto.AssemblyName ||
             oldQuartzNet.ClassName != createUpdateQuartzNetDto.ClassName) && await TableWhere(q =>
                q.AssemblyName == createUpdateQuartzNetDto.AssemblyName &&
                q.ClassName == createUpdateQuartzNetDto.ClassName).AnyAsync())
        {
            return OperateResult.Error(
                ValidationError.IsExist(createUpdateQuartzNetDto, nameof(createUpdateQuartzNetDto.ClassName)));
        }

        var quartzNet = App.Mapper.MapTo<QuartzNet>(createUpdateQuartzNetDto);
        var result = await UpdateAsync(quartzNet);
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 更新作业
    /// </summary>
    /// <param name="quartzNet"></param>
    /// <returns></returns>
    public async Task<OperateResult> UpdateJobInfoAsync(QuartzNet quartzNet)
    {
        await UpdateAsync(quartzNet);
        return OperateResult.Success();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="quartzNets"></param>
    /// <returns></returns>
    public async Task<OperateResult> DeleteAsync(List<QuartzNet> quartzNets)
    {
        var ids = quartzNets.Select(x => x.Id).ToList();
        var result = await LogicDelete<QuartzNet>(x => ids.Contains(x.Id));
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<QuartzNetVo>> QueryAsync(QuartzNetQueryCriteria quartzNetQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<QuartzNet>
        {
            Pagination = pagination,
            ConditionalModels = quartzNetQueryCriteria.ApplyQueryConditionalModel()
        };
        return App.Mapper.MapTo<List<QuartzNetVo>>(
            await TablePageAsync(queryOptions));
    }

    /// <summary>
    /// 查询全部作业名称
    /// </summary>
    /// <returns></returns>
    public async Task<List<QuartzNetSmallVo>> QueryAllTaskNameAsync()
    {
        return App.Mapper.MapTo<List<QuartzNetSmallVo>>(await Table.ToListAsync());
    }

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="quartzNetQueryCriteria"></param>
    /// <returns></returns>
    public async Task<List<ExportBase>> DownloadAsync(QuartzNetQueryCriteria quartzNetQueryCriteria)
    {
        var quartzNets = await TableWhere(quartzNetQueryCriteria.ApplyQueryConditionalModel()).ToListAsync();
        List<ExportBase> quartzExports = new List<ExportBase>();
        quartzExports.AddRange(quartzNets.Select(x => new QuartzNetExport
        {
            Id = x.Id,
            TaskName = x.TaskName,
            TaskGroup = x.TaskGroup,
            Cron = x.Cron,
            AssemblyName = x.AssemblyName,
            ClassName = x.ClassName,
            Description = x.Description,
            Principal = x.Principal,
            AlertEmail = x.AlertEmail,
            PauseAfterFailure = x.PauseAfterFailure,
            RunTimes = x.RunTimes,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            TriggerType = x.TriggerType,
            IntervalSecond = x.IntervalSecond,
            CycleRunTimes = x.CycleRunTimes,
            Enabled = x.Enabled,
            RunParams = x.RunParams,
            CreateTime = x.CreateTime
        }));
        return quartzExports;
    }

    #endregion
}
