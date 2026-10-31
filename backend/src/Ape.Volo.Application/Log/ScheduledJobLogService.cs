using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Log;

namespace Ape.Volo.Application.Log;

/// <summary>
/// QuartzNet作业日志服务
/// </summary>
public class ScheduledJobLogService : BaseService<ScheduledJobLog>, IScheduledJobLogService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public ScheduledJobLogService(ISugarRepository<ScheduledJobLog> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    public async Task<OperateResult> CreateAsync(ScheduledJobLogDto scheduledJobLogDto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var scheduledJobLog = Mapper.MapTo<ScheduledJobLog>(scheduledJobLogDto);
        // 分表插入没有取消重载，必须等待写入结束后再让所属作用域释放。
        var result = await SugarClient.Insertable(scheduledJobLog).SplitTable().ExecuteCommandAsync() > 0;
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ScheduledJobLogs.List)]
    public Task<PagedResultDto<ScheduledJobLogVo>> GetListAsync(
        ScheduledJobLogQueryCriteria scheduledJobLogQueryCriteria, CancellationToken ct = default)
        => PageAsync<ScheduledJobLogVo>(scheduledJobLogQueryCriteria, options => options.IsSplitTable = true, ct);

    /// <inheritdoc />
    [HasRole(RoleCodes.Admin)]
    public async Task<List<ExportBase>> DownloadAsync(ScheduledJobLogQueryCriteria scheduledJobLogQueryCriteria, CancellationToken ct = default)
    {
        var scheduledJobLogs = await TableWhere(scheduledJobLogQueryCriteria.ApplyQueryConditionalModel(), isSplitTable: true)
            .ToListAsync(ct);
        List<ExportBase> scheduledJobLogExports = new List<ExportBase>();
        scheduledJobLogExports.AddRange(Mapper.MapTo<List<ScheduledJobLogExport>>(scheduledJobLogs));
        return scheduledJobLogExports;
    }

    #endregion

}
