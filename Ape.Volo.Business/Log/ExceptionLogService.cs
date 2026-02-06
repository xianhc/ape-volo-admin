using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Entity.Log;
using Ape.Volo.IBusiness.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.Monitor;
using SqlSugar;

namespace Ape.Volo.Business.Log;

/// <summary>
/// 系统日志服务
/// </summary>
public class ExceptionLogService : BaseServices<ExceptionLog>, IExceptionLogService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="exceptionLog"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(ExceptionLog exceptionLog)
    {
        var result = await SugarRepository.SugarClient.Insertable(exceptionLog).SplitTable().ExecuteCommandAsync() > 0;
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<ExceptionLogVo>> QueryAsync(LogQueryCriteria logQueryCriteria, Pagination pagination)
    {
        var queryOptions = new QueryOptions<ExceptionLog>
        {
            Pagination = pagination,
            ConditionalModels = logQueryCriteria.ApplyQueryConditionalModel(),
            IsSplitTable = true
        };
        var logs = await TablePageAsync(queryOptions);
        return App.Mapper.MapTo<List<ExceptionLogVo>>(logs);
    }

    /// <summary>
    /// 获取异常日志数量
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    public async Task<List<int>> GetOperationNumber(int days = 7)
    {
        DateTime startDate = DateTime.Now.AddDays(-(days - 1));


        var dateList = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i).ToString("yyyy-MM-dd"))
            .ToList();


        var list = await Table.SplitTable().Where(x => x.CreateTime >= startDate)
            .GroupBy(x => x.CreateTime.ToString("yyyy-MM-dd"))
            .Select(it => new { Time = it.CreateTime.ToString("yyyy-MM-dd"), Count = SqlFunc.AggregateCount(it.Id) })
            .OrderBy(it => it.Time)
            .ToListAsync();

        var dict = list.ToDictionary(x => x.Time, x => x.Count);

        var numbers = dateList.Select(date => dict.ContainsKey(date) ? dict[date] : 0).ToList();

        return numbers;
    }

    #endregion
}