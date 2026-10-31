namespace Ape.Volo.Application.Contracts.Log.Models;

/// <summary>
/// 任务调度日志Vo
/// </summary>
public class ScheduledJobLogVo : BaseEntityDto<long>
{
    /// <summary>
    /// 创建者账号，日志列表直接展示
    /// </summary>
    public string? CreatorName { get; set; }

    /// <summary>
    /// 任务ID
    /// </summary>
    public long ScheduledJobId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// 任务分组
    /// </summary>
    public string? JobGroup { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string? AssemblyName { get; set; }

    /// <summary>
    /// 任务所在类
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// cron 表达式
    /// </summary>
    public string? Cron { get; set; }

    /// <summary>
    /// 异常详情
    /// </summary>
    public string? ExceptionDetail { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long ExecutionDuration { get; set; }

    /// <summary>
    /// 执行传参
    /// </summary>
    public string? RunParams { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
}