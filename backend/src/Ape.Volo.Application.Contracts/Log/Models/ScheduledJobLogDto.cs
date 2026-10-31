namespace Ape.Volo.Application.Contracts.Log.Models;

/// <summary>
/// 作业日志Dto
/// </summary>
public class ScheduledJobLogDto : BaseEntityDto<long>
{
    /// <summary>
    /// 创建者账号，日志列表直接展示
    /// </summary>
    public string? CreatorName { get; set; }

    /// <summary>
    /// 作业ID
    /// </summary>
    public long ScheduledJobId { get; set; }

    /// <summary>
    /// 作业名称
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// 作业组
    /// </summary>
    public string? JobGroup { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string? AssemblyName { get; set; }

    /// <summary>
    /// 类名
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Cron表达式
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
    /// 运行参数
    /// </summary>
    public string? RunParams { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
}
