using System.Collections.Generic;
using System.Threading.Tasks;
using Ape.Volo.Common.Model;
using Ape.Volo.Entity.Log;
using Ape.Volo.SharedModel.Queries.Common;
using Ape.Volo.SharedModel.Queries.System;
using Ape.Volo.ViewModel.Core.Monitor;

namespace Ape.Volo.IBusiness.Log;

/// <summary>
/// 系统日志接口
/// </summary>
public interface IExceptionLogService : IBaseServices<ExceptionLog>
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="exceptionLog"></param>
    /// <returns></returns>
    Task<OperateResult> CreateAsync(ExceptionLog exceptionLog);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="pagination"></param>
    /// <returns></returns>
    Task<List<ExceptionLogVo>> QueryAsync(LogQueryCriteria logQueryCriteria, Pagination pagination);


    /// <summary>
    /// 获取异常日志数量
    /// </summary>
    /// <param name="days"></param>
    /// <returns></returns>
    Task<List<int>> GetOperationNumber(int days = 7);

    #endregion
}