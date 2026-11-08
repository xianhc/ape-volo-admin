using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Log;

namespace Ape.Volo.Application.Log;

/// <summary>
/// QuartzNet作业日志服务
/// </summary>
public class JobLogService : BaseService<JobLog>, IJobLogService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public JobLogService(ISugarRepository<JobLog> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    [HasPerm(SystemPermissions.JobLogs.List)]
    public Task<PagedResultDto<JobLogVo>> GetListAsync(
        JobLogQueryCriteria jobLogQueryCriteria, CancellationToken ct = default)
        => PageAsync<JobLogVo>(jobLogQueryCriteria, options => options.IsSplitTable = true, ct);

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<List<ExportBase>> DownloadAsync(JobLogQueryCriteria jobLogQueryCriteria, CancellationToken ct = default)
    {
        var jobLogs = await TableWhere(jobLogQueryCriteria.ApplyQueryConditionalModel(), isSplitTable: true)
            .ToListAsync(ct);
        List<ExportBase> jobLogExports = new List<ExportBase>();
        jobLogExports.AddRange(Mapper.MapTo<List<JobLogExport>>(jobLogs));
        return jobLogExports;
    }

    #endregion

}
