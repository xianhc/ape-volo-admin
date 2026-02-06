using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Model;
using Ape.Volo.Core;
using Ape.Volo.Entity.Log;
using Ape.Volo.IBusiness.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.LogInfo;
using SqlSugar;

namespace Ape.Volo.Business.Log;

/// <summary>
/// 操作日志服务
/// </summary>
public class OperateLogService : BaseServices<OperateLog>, IOperateLogService
{
    #region 基础方法

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="operateLog"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateAsync(OperateLog operateLog)
    {
        var result = await SugarRepository.SugarClient.Insertable(operateLog).SplitTable().ExecuteCommandAsync() > 0;
        return OperateResult.Result(result);
    }

    /// <summary>
    /// 批量创建
    /// </summary>
    /// <param name="operateLogs"></param>
    /// <returns></returns>
    public async Task<OperateResult> CreateListAsync(List<OperateLog> operateLogs)
    {
        var result = await SugarRepository.SugarClient.Insertable(operateLogs).SplitTable().ExecuteCommandAsync() > 0;
        return OperateResult.Result(result);
    }



    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<OperateLogVo>> QueryAsync(LogQueryCriteria logQueryCriteria,
        Pagination pagination)
    {
        var queryOptions = new QueryOptions<OperateLog>
        {
            Pagination = pagination,
            ConditionalModels = logQueryCriteria.ApplyQueryConditionalModel(),
            IsSplitTable = true
        };

        var operateLogs = await TablePageAsync(queryOptions);
        return App.Mapper.MapTo<List<OperateLogVo>>(operateLogs);
    }

    /// <summary>
    /// 查询当前用户
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    public async Task<List<OperateLogVo>> QueryByCurrentAsync(Pagination pagination)
    {
        Expression<Func<OperateLog, bool>> whereLambda = x => x.CreateBy == App.HttpUser.Account;
        var queryOptions = new QueryOptions<OperateLog>
        {
            Pagination = pagination,
            WhereLambda = whereLambda,
            IsSplitTable = true
        };
        var operateLogs = await TablePageAsync(queryOptions);
        return App.Mapper.MapTo<List<OperateLogVo>>(operateLogs);
    }

    /// <summary>
    /// 获取操作日志数量
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<int>> GetOperationNumber(int days = 7)
    {
        DateTime startDate = DateTime.Now.AddDays(-(days - 1));

        var dateList = Enumerable.Range(0, days)
            .Select(i => startDate.AddDays(i).ToString("yyyy-MM-dd"))
            .ToList();


        var list = await Table.SplitTable().Where(x => x.CreateTime >= startDate)
        .GroupBy(x => x.CreateTime.ToString("yyyy-MM-dd"))
        .Select(it => new
        {
            Time = it.CreateTime.ToString("yyyy-MM-dd"),
            Count = SqlFunc.AggregateCount(it.Id)

        })
        .OrderBy(it => it.Time)
        .ToListAsync();

        var dict = list.ToDictionary(x => x.Time, x => x.Count);

        var numbers = dateList.Select(date => dict.ContainsKey(date) ? dict[date] : 0).ToList();

        return numbers;
    }



    #endregion
}
