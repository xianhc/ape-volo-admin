namespace Ape.Volo.Web.Quartz;

/// <summary>
/// 宿主拥有的调度中心，业务只依赖 IJobScheduler 端口。
/// </summary>
public interface ISchedulerCenterService : IJobScheduler
{
    /// <summary>
    /// 启动调度器，已经启动时也视为成功。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task<bool> StartScheduleAsync(CancellationToken ct = default);
    /// <summary>
    /// 关闭已创建的调度器并等待当前作业结束；未创建时无副作用。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task<bool> ShutdownScheduleAsync(CancellationToken ct = default);
}
