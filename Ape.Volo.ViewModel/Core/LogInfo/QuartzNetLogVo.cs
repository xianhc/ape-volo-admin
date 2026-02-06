using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Log;

namespace Ape.Volo.ViewModel.Core.System;

/// <summary>
/// 任务调度日志Vo
/// </summary>
[AutoMapping(typeof(QuartzNetLog), typeof(QuartzNetLogVo))]
public class QuartzNetLogVo : BaseEntityDto<long>
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public long TaskId { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; }

    /// <summary>
    /// 任务分组
    /// </summary>
    public string TaskGroup { get; set; }

    /// <summary>
    /// 程序集名称
    /// </summary>
    public string AssemblyName { get; set; }

    /// <summary>
    /// 任务所在类
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// cron 表达式
    /// </summary>
    public string Cron { get; set; }

    /// <summary>
    /// 异常详情
    /// </summary>
    public string ExceptionDetail { get; set; }

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long ExecutionDuration { get; set; }

    /// <summary>
    /// 执行传参
    /// </summary>
    public string RunParams { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }
}