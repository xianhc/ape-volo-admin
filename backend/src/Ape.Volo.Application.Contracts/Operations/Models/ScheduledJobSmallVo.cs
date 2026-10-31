namespace Ape.Volo.Application.Contracts.Operations.Models;

/// <summary>
/// 任务调度Vo
/// </summary>
public class ScheduledJobSmallVo
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string? JobName { get; set; }
}
