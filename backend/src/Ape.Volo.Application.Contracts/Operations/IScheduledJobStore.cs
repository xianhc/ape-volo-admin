using Ape.Volo.Application.Contracts.Operations.Models;
using Ape.Volo.Shared.Model;

namespace Ape.Volo.Application.Contracts.Operations;

/// <summary>
/// 调度编排、启动加载和作业执行使用的内部配置存取，不生成远程接口。
/// </summary>
public interface IScheduledJobStore
{
    /// <summary>读取指定作业。</summary>
    /// <param name="id">作业 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业配置</returns>
    Task<ScheduledJobVo> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>批量读取作业。</summary>
    /// <param name="ids">作业 ID 集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业配置集合</returns>
    Task<List<ScheduledJobVo>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default);

    /// <summary>读取启动加载的全部作业。</summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>作业配置集合</returns>
    Task<List<ScheduledJobVo>> GetAllAsync(CancellationToken ct = default);

    /// <summary>保存新作业，调用者负责同步运行时调度。</summary>
    /// <param name="request">作业配置</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>保存后的作业</returns>
    Task<ScheduledJobVo> CreateAsync(CreateUpdateScheduledJobDto request, CancellationToken ct = default);

    /// <summary>修改作业，调用者负责同步运行时调度。</summary>
    /// <param name="request">作业配置</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>保存结果</returns>
    Task<OperateResult> UpdateAsync(CreateUpdateScheduledJobDto request, CancellationToken ct = default);

    /// <summary>保存启用状态等运行配置。</summary>
    /// <param name="job">作业配置</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>保存结果</returns>
    Task<OperateResult> UpdateJobInfoAsync(ScheduledJobVo job, CancellationToken ct = default);

    /// <summary>删除已停止调度的作业配置。</summary>
    /// <param name="ids">作业 ID 集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>删除结果</returns>
    Task<OperateResult> DeleteAsync(HashSet<long> ids, CancellationToken ct = default);
}
