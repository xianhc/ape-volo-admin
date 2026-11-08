using Ape.Volo.Application.Contracts.System.Jobs.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.System.Jobs;

/// <summary>
/// Job作业接口
/// </summary>
public interface IJobService : IApplicationService
{
    #region 基础接口

    /// <summary>
    /// 按主键查询单条
    /// </summary>
    /// <param name="id">主键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业</returns>
    /// <exception cref="Ape.Volo.Shared.Exceptions.BadRequestException">作业不存在</exception>
    // 仅供进程内 IJobStore 读取，不对外暴露。
    [RemoteService(false)]
    Task<JobVo> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 按主键批量查询
    /// </summary>
    /// <param name="ids">主键集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业集合，主键集合为空时返回空集合</returns>
    [RemoteService(false)]
    Task<List<JobVo>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default);

    /// <summary>
    /// 查询全部
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<JobVo>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// 创建
    /// </summary>
    /// <param name="createUpdateJobDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns>创建后的作业</returns>
    // 写入必须经 IJobManager 同步调度，禁止绕过编排直接修改配置。
    [RemoteService(false)]
    Task<JobVo> CreateAsync(CreateUpdateJobDto createUpdateJobDto, CancellationToken ct = default);

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="createUpdateJobDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> UpdateAsync(CreateUpdateJobDto createUpdateJobDto, CancellationToken ct = default);

    /// <summary>
    /// 保存作业信息（用于启用/停用等状态变更）
    /// </summary>
    /// <param name="jobVo"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [RemoteService(false)]
    Task<OperateResult> UpdateJobInfoAsync(JobVo jobVo, CancellationToken ct = default);

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
    /// <param name="jobQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<PagedResultDto<JobVo>> GetListAsync(JobQueryCriteria jobQueryCriteria, CancellationToken ct = default);

    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="jobQueryCriteria"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    Task<List<ExportBase>> DownloadAsync(JobQueryCriteria jobQueryCriteria, CancellationToken ct = default);

    #endregion
}
