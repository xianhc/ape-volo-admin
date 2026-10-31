using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.Operations;

namespace Ape.Volo.Application.Operations;

/// <summary>
/// QuartzNet作业服务
/// </summary>
public class ScheduledJobService : BaseService<ScheduledJob>, IScheduledJobService, IScheduledJobStore
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public ScheduledJobService(ISugarRepository<ScheduledJob> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    Task<ScheduledJobVo> IScheduledJobStore.QueryByIdAsync(long id, CancellationToken ct) => QueryByIdAsync(id, ct);

    /// <inheritdoc />
    Task<List<ScheduledJobVo>> IScheduledJobStore.QueryByIdsAsync(IEnumerable<long> ids, CancellationToken ct) => QueryByIdsAsync(ids, ct);

    /// <inheritdoc />
    Task<List<ScheduledJobVo>> IScheduledJobStore.GetAllAsync(CancellationToken ct) => GetAllAsync(ct);

    /// <inheritdoc />
    /// <exception cref="BadRequestException">作业不存在</exception>
    [HasPerm(SystemPermissions.ScheduledJobs.List)]
    public async Task<ScheduledJobVo> QueryByIdAsync(long id, CancellationToken ct = default)
    {
        var scheduledJob = await GetByIdAsync(id, ct);

        // 按主键查询属于「必须存在」的语义，缺数据直接抛业务异常，避免调用方到处判空
        if (scheduledJob == null)
        {
            throw new BadRequestException("作业不存在");
        }

        return Mapper.MapTo<ScheduledJobVo>(scheduledJob);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.List)]
    public async Task<List<ScheduledJobVo>> QueryByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        var scheduledJobs = await GetByIdsAsync(ids, ct);
        return Mapper.MapTo<List<ScheduledJobVo>>(scheduledJobs);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.List, SystemPermissions.ScheduledJobLogs.List)]
    public async Task<List<ScheduledJobVo>> GetAllAsync(CancellationToken ct = default)
    {
        return Mapper.MapTo<List<ScheduledJobVo>>(await Table.ToListAsync(ct));
    }

    /// <inheritdoc />
    public async Task<ScheduledJobVo> CreateAsync(CreateUpdateScheduledJobDto createUpdateScheduledJobDto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (await TableWhere(q => q.JobName == createUpdateScheduledJobDto.JobName).CountAsync(ct) > 0)
        {
            throw new BadRequestException(
                ValidationError.IsExist(createUpdateScheduledJobDto, nameof(createUpdateScheduledJobDto.JobName)));
        }

        if (await TableWhere(q =>
                q.AssemblyName == createUpdateScheduledJobDto.AssemblyName &&
                q.ClassName == createUpdateScheduledJobDto.ClassName).CountAsync(ct) > 0)
        {
            throw new BadRequestException(
                ValidationError.IsExist(createUpdateScheduledJobDto, nameof(createUpdateScheduledJobDto.ClassName)));
        }

        var scheduledJob = Mapper.MapTo<ScheduledJob>(createUpdateScheduledJobDto);
        var written = await SugarClient.Insertable(scheduledJob).ExecuteCommandAsync(ct);
        if (written <= 0) throw new BadRequestException("作业保存失败。");
        return Mapper.MapTo<ScheduledJobVo>(scheduledJob);
    }

    /// <inheritdoc />
    public async Task<OperateResult> UpdateAsync(CreateUpdateScheduledJobDto createUpdateScheduledJobDto, CancellationToken ct = default)
    {
        var oldScheduledJob =
            await TableWhere(x => x.Id == createUpdateScheduledJobDto.Id).FirstAsync(ct);
        if (oldScheduledJob.IsNull())
        {
            return OperateResult.Error(ValidationError.NotExist(createUpdateScheduledJobDto,
                "作业调度",
                nameof(createUpdateScheduledJobDto.Id)));
        }

        if (oldScheduledJob.JobName != createUpdateScheduledJobDto.JobName
            && await TableWhere(q =>
                q.JobName == createUpdateScheduledJobDto.JobName).CountAsync(ct) > 0)
        {
            return OperateResult.Error(
                ValidationError.IsExist(createUpdateScheduledJobDto, nameof(createUpdateScheduledJobDto.JobName)));
        }


        if ((oldScheduledJob.AssemblyName != createUpdateScheduledJobDto.AssemblyName ||
             oldScheduledJob.ClassName != createUpdateScheduledJobDto.ClassName) && await TableWhere(q =>
                q.AssemblyName == createUpdateScheduledJobDto.AssemblyName &&
                q.ClassName == createUpdateScheduledJobDto.ClassName).CountAsync(ct) > 0)
        {
            return OperateResult.Error(
                ValidationError.IsExist(createUpdateScheduledJobDto, nameof(createUpdateScheduledJobDto.ClassName)));
        }

        var scheduledJob = Mapper.MapTo<ScheduledJob>(createUpdateScheduledJobDto);
        var result = await UpdateAsync(scheduledJob, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    public async Task<OperateResult> UpdateJobInfoAsync(ScheduledJobVo scheduledJobVo, CancellationToken ct = default)
    {
        var scheduledJob = Mapper.MapTo<ScheduledJob>(scheduledJobVo);
        return OperateResult.Result(await UpdateAsync(scheduledJob, ct: ct));
    }

    /// <inheritdoc />
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.List)]
    public Task<PagedResultDto<ScheduledJobVo>> GetListAsync(ScheduledJobQueryCriteria scheduledJobQueryCriteria, CancellationToken ct = default)
        => PageAsync<ScheduledJobVo>(scheduledJobQueryCriteria, ct: ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.List, SystemPermissions.ScheduledJobLogs.List)]
    public async Task<List<ScheduledJobSmallVo>> QueryAllJobNameAsync(CancellationToken ct = default)
    {
        return Mapper.MapTo<List<ScheduledJobSmallVo>>(await Table.ToListAsync(ct));
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.List)]
    public async Task<ScheduledJobVo?> QueryByJobNameAsync(string jobName, CancellationToken ct = default)
    {
        var scheduledJob = await TableWhere(x => x.JobName == jobName).FirstAsync(ct);
        return scheduledJob == null ? null : Mapper.MapTo<ScheduledJobVo>(scheduledJob);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobs.Download)]
    public async Task<List<ExportBase>> DownloadAsync(ScheduledJobQueryCriteria scheduledJobQueryCriteria, CancellationToken ct = default)
    {
        var scheduledJobs = await TableWhere(scheduledJobQueryCriteria.ApplyQueryConditionalModel()).ToListAsync(ct);
        List<ExportBase> scheduledJobExports = new List<ExportBase>();
        scheduledJobExports.AddRange(scheduledJobs.Select(x => new ScheduledJobExport
        {
            Id = x.Id,
            JobName = x.JobName,
            JobGroup = x.JobGroup,
            Cron = x.Cron!,
            AssemblyName = x.AssemblyName,
            ClassName = x.ClassName,
            Description = x.Description!,
            Principal = x.Principal!,
            AlertEmail = x.AlertEmail!,
            PauseAfterFailure = x.PauseAfterFailure,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            TriggerType = x.TriggerType,
            IntervalSecond = x.IntervalSecond,
            CycleRunTimes = x.CycleRunTimes,
            Enabled = x.Enabled,
            RunParams = x.RunParams!,
            CreationTime = x.CreationTime
        }));
        return scheduledJobExports;
    }

    #endregion

}
