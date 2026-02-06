using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.LogInfo;

namespace Ape.Volo.IBusiness.Log;

/// <summary>
/// 操作日志接口
/// </summary>
public interface IOperateLogService : IBaseServices<OperateLog>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="operateLog"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(OperateLog operateLog);


    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="operateLogs"></param>
    /// <returns></returns>
    Task<OperateResult> CreateListAsync(List<OperateLog> operateLogs);


    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<OperateLogVo>> QueryAsync(LogQueryCriteria logQueryCriteria, Pagination pagination);

    /// <summary>
    /// 查询(个人)
    /// </summary>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<OperateLogVo>> QueryByCurrentAsync(Pagination pagination);

    /// <summary>
    /// 获取操作日志数量
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    Task<List<int>> GetOperationNumber(int days = 7);

    #endregion
}