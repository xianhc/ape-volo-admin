using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Shared.Exceptions;
using Ape.Volo.Application.Contracts.System.Jobs.Models;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Utils;
using Ape.Volo.Entity.System;

namespace Ape.Volo.Application.System.Jobs;

/// <summary>
/// QuartzNet作业服务
/// </summary>
public class JobService : BaseService<Job>, IJobService, IJobStore
{
    private readonly IJobScheduler _scheduler;

    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    /// <param name="scheduler">运行时调度状态查询</param>
    public JobService(ISugarRepository<Job> sugarRepository, IMapper mapper, IJobScheduler scheduler) : base(sugarRepository, mapper)
    {
        _scheduler = scheduler;
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    Task<JobVo> IJobStore.GetByIdAsync(long id, CancellationToken ct) => GetByIdAsync(id, ct);

    /// <inheritdoc />
    Task<List<JobVo>> IJobStore.GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct) => GetByIdsAsync(ids, ct);

    /// <inheritdoc />
    Task<List<JobVo>> IJobStore.GetAllAsync(CancellationToken ct) => GetAllAsync(ct);

    /// <inheritdoc />
    /// <exception cref="BadRequestException">作业不存在</exception>
    [HasPerm(SystemPermissions.Jobs.List)]
    public async Task<JobVo> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var job = await GetEntityByIdAsync(id, ct);

        // 按主键查询属于「必须存在」的语义，缺数据直接抛业务异常，避免调用方到处判空
        if (job == null)
        {
            throw new BadRequestException("作业不存在");
        }

        return Mapper.MapTo<JobVo>(job);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Jobs.List)]
    public async Task<List<JobVo>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        var jobs = await GetEntitiesByIdsAsync(ids, ct);
        return Mapper.MapTo<List<JobVo>>(jobs);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Jobs.List, SystemPermissions.JobLogs.List)]
    public async Task<List<JobVo>> GetAllAsync(CancellationToken ct = default)
    {
        return Mapper.MapTo<List<JobVo>>(await Table.ToListAsync(ct));
    }

    /// <inheritdoc />
    public async Task<JobVo> CreateAsync(CreateUpdateJobDto createUpdateJobDto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (await TableWhere(q => q.JobName == createUpdateJobDto.JobName).CountAsync(ct) > 0)
        {
            throw new BadRequestException(
                ValidationError.IsExist(() => createUpdateJobDto.JobName, typeof(Job)));
        }

        if (await TableWhere(q =>
                q.AssemblyName == createUpdateJobDto.AssemblyName &&
                q.ClassName == createUpdateJobDto.ClassName).CountAsync(ct) > 0)
        {
            throw new BadRequestException(
                ValidationError.IsExist(() => createUpdateJobDto.ClassName, typeof(Job)));
        }

        var job = Mapper.MapTo<Job>(createUpdateJobDto);
        var written = await SugarClient.Insertable(job).ExecuteCommandAsync(ct);
        if (written <= 0) throw new BadRequestException("作业保存失败。");
        return Mapper.MapTo<JobVo>(job);
    }

    /// <inheritdoc />
    public async Task<OperateResult> UpdateAsync(CreateUpdateJobDto createUpdateJobDto, CancellationToken ct = default)
    {
        var oldJob =
            await TableWhere(x => x.Id == createUpdateJobDto.Id).FirstAsync(ct);
        if (oldJob.IsNull())
        {
            return NotFound(createUpdateJobDto.Id);
        }

        if (oldJob.JobName != createUpdateJobDto.JobName
            && await TableWhere(q =>
                q.JobName == createUpdateJobDto.JobName).CountAsync(ct) > 0)
        {
            return AlreadyExists(() => createUpdateJobDto.JobName);
        }


        if ((oldJob.AssemblyName != createUpdateJobDto.AssemblyName ||
             oldJob.ClassName != createUpdateJobDto.ClassName) && await TableWhere(q =>
                q.AssemblyName == createUpdateJobDto.AssemblyName &&
                q.ClassName == createUpdateJobDto.ClassName).CountAsync(ct) > 0)
        {
            return AlreadyExists(() => createUpdateJobDto.ClassName);
        }

        var job = Mapper.MapTo<Job>(createUpdateJobDto);
        var result = await UpdateAsync(job, ct: ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    public async Task<OperateResult> UpdateJobInfoAsync(JobVo jobVo, CancellationToken ct = default)
    {
        var job = Mapper.MapTo<Job>(jobVo);
        return OperateResult.Result(await UpdateAsync(job, ct: ct));
    }

    /// <inheritdoc />
    public async Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default)
    {
        var result = await LogicDeleteAsync(x => ids.Contains(x.Id), ct);
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Jobs.List)]
    public async Task<PagedResultDto<JobVo>> GetListAsync(JobQueryCriteria jobQueryCriteria, CancellationToken ct = default)
    {
        var page = await PageAsync<JobVo>(jobQueryCriteria, ct: ct);
        foreach (var job in page.Items)
            job.TriggerState = await _scheduler.GetTriggerStatus(job.Id, ct);
        return page;
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Jobs.Download)]
    public async Task<List<ExportBase>> DownloadAsync(JobQueryCriteria jobQueryCriteria, CancellationToken ct = default)
    {
        var jobs = await TableWhere(jobQueryCriteria.ApplyQueryConditionalModel()).ToListAsync(ct);
        List<ExportBase> jobExports = new List<ExportBase>();
        jobExports.AddRange(jobs.Select(x => new JobExport
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
        return jobExports;
    }

    #endregion

}
