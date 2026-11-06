using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Log;

/// <summary>
/// 系统日志接口
/// </summary>
public interface IExceptionLogService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<ExceptionLogVo>> GetListAsync(LogQueryCriteria logQueryCriteria, CancellationToken ct = default);

    #endregion
}
