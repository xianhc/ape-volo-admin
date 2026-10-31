using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Operations;

/// <summary>
/// ScheduledJob作业接口
/// </summary>
public interface IScheduledJobService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 按主键查询单条
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业</returns>
    /// <exception cref="Ape.Volo.Shared.Exceptions.BadRequestException">作业不存在</exception>
    Task<ScheduledJobVo> QueryByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 按主键批量查询
    /// </summary>
    /// <param name="ids">主键集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业集合，主键集合为空时返回空集合</returns>
    Task<List<ScheduledJobVo>> QueryByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ScheduledJobVo>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateScheduledJobDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns>创建后的作业</returns>
    // 写入必须经 IScheduledJobManager 同步调度，禁止绕过编排直接修改配置。
    [RemoteService(false)]
    Task<ScheduledJobVo> CreateAsync(CreateUpdateScheduledJobDto createUpdateScheduledJobDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateScheduledJobDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> UpdateAsync(CreateUpdateScheduledJobDto createUpdateScheduledJobDto, CancellationToken ct = default);

    /// <summary>
    /// 保存作业信息（用于启用/停用等状态变更）
    /// </summary>
    /// <param name="scheduledJobVo"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> UpdateJobInfoAsync(ScheduledJobVo scheduledJobVo, CancellationToken ct = default);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="ids">作业ID集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询
    /// </summary>
    /// <param name="scheduledJobQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<ScheduledJobVo>> GetListAsync(ScheduledJobQueryCriteria scheduledJobQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 查询全部作业名称
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ScheduledJobSmallVo>> QueryAllJobNameAsync(CancellationToken ct = default);

    /// <summary>
    /// 按作业名称查询
    /// </summary>
    /// <param name="jobName">作业名称</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业，不存在时返回 null</returns>
    Task<ScheduledJobVo?> QueryByJobNameAsync(string jobName, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="scheduledJobQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(ScheduledJobQueryCriteria scheduledJobQueryCriteria, CancellationToken ct = default);

    #endregion
}
