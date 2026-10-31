using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Shared.Model;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Entity.Log;
using SqlSugar;

namespace Ape.Volo.Application.Log;

/// <summary>
/// 系统日志服务
/// </summary>
public class ExceptionLogService : BaseService<ExceptionLog>, IExceptionLogService
{
    #region 构造函数

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sugarRepository">当前实体的仓储</param>
    /// <param name="mapper">对象映射器</param>
    public ExceptionLogService(ISugarRepository<ExceptionLog> sugarRepository, IMapper mapper) : base(sugarRepository, mapper)
    {
    }

    #endregion

    #region 基础方法

    /// <inheritdoc />
    public async Task<OperateResult> CreateAsync(ExceptionLogDto exceptionLogDto, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var exceptionLog = Mapper.MapTo<ExceptionLog>(exceptionLogDto);
        // 分表插入没有取消重载，必须等待写入结束后再让所属作用域释放。
        var result = await SugarClient.Insertable(exceptionLog).SplitTable().ExecuteCommandAsync() > 0;
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    public async Task<OperateResult> CreateListAsync(List<ExceptionLogDto> exceptionLogDtos, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var exceptionLogs = Mapper.MapTo<List<ExceptionLog>>(exceptionLogDtos);
        var result = await SugarClient.Insertable(exceptionLogs).SplitTable().ExecuteCommandAsync() > 0;
        return OperateResult.Result(result);
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ExceptionLogs.List)]
    public Task<PagedResultDto<ExceptionLogVo>> GetListAsync(LogQueryCriteria logQueryCriteria, CancellationToken ct = default)
        => PageAsync<ExceptionLogVo>(logQueryCriteria, options => options.IsSplitTable = true, ct);

    /// <inheritdoc />
    [HasPerm(SystemPermissions.ExceptionLogs.List)]
    public async Task<List<int>> GetOperationNumber(int days = 7, CancellationToken ct = default)
    {
        DateTime startDate = DateTime.Now.AddDays(-(days - 1));


        var dateList = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i).ToString("yyyy-MM-dd"))
            .ToList();


        var list = await Table.SplitTable().Where(x => x.CreationTime >= startDate)
            .GroupBy(x => x.CreationTime.ToString("yyyy-MM-dd"))
            .Select(it => new { Time = it.CreationTime.ToString("yyyy-MM-dd"), Count = SqlFunc.AggregateCount(it.Id) })
            .OrderBy(it => it.Time)
            .ToListAsync(ct);

        var dict = list.ToDictionary(x => x.Time, x => x.Count);

        var numbers = dateList.Select(date => dict.ContainsKey(date) ? dict[date] : 0).ToList();

        return numbers;
    }

    #endregion

}
