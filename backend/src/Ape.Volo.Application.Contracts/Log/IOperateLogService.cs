using Ape.Volo.Application.Contracts.Log.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Log;

/// <summary>
/// 操作日志接口
/// </summary>
public interface IOperateLogService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="operateLogDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    // 审计日志由宿主生成，不接受客户端伪造写入。
    [RemoteService(false)]
    Task<OperateResult> CreateAsync(OperateLogDto operateLogDto, CancellationToken ct = default);


    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="operateLogDtos"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> CreateListAsync(List<OperateLogDto> operateLogDtos, CancellationToken ct = default);


    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="logQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<OperateLogVo>> GetListAsync(LogQueryCriteria logQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 查询(个人)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<OperateLogVo>> QueryByCurrentAsync(PagedAndSortedResultRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// 获取操作日志数量
    /// </summary>
    /// <param name="days"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<int>> GetOperationNumber(int days = 7, CancellationToken ct = default);

    #endregion
}
