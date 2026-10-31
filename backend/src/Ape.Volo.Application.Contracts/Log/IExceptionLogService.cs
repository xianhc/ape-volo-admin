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
    /// 创建
    /// </summary>
    /// <param name="exceptionLogDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    // 日志只能由宿主写入，管理接口仅提供查询。
    [RemoteService(false)]
    Task<OperateResult> CreateAsync(ExceptionLogDto exceptionLogDto, CancellationToken ct = default);

    /// <summary>
    /// 批量创建
    /// </summary>
    /// <param name="exceptionLogDtos"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> CreateListAsync(List<ExceptionLogDto> exceptionLogDtos, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<ExceptionLogVo>> GetListAsync(LogQueryCriteria logQueryCriteria, CancellationToken ct = default);


    /// <summary>
    /// 获取异常日志数量
    /// </summary>
    /// <param name="days"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<int>> GetOperationNumber(int days = 7, CancellationToken ct = default);

    #endregion
}
