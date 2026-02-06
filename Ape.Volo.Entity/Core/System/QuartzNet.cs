using System;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using Quartz;
using SqlSugar;

namespace Ape.Volo.Entity.Core.System
{
    /// <summary>
    /// 系统作业调度
    /// </summary>
    [SugarTable("sys_quartz_job")]
    public class QuartzNet : BaseEntity
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TaskName { get; set; } = string.Empty;

        /// <summary>
        /// 任务分组
        /// </summary>
        public string TaskGroup { get; set; } = string.Empty;

        /// <summary>
        /// cron 表达式
        /// </summary>
        public string? Cron { get; set; }

        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName { get; set; } = string.Empty;

        /// <summary>
        /// 任务所在类
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// 任务描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 任务负责人
        /// </summary>
        public string? Principal { get; set; }

        /// <summary>
        /// 告警邮箱
        /// </summary>
        public string? AlertEmail { get; set; }

        /// <summary>
        /// 任务失败后是否暂停
        /// </summary>
        public bool PauseAfterFailure { get; set; }

        /// <summary>
        /// 执行次数
        /// </summary>
        public int RunTimes { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 触发器类型（0、simple 1、cron）
        /// </summary>
        public TriggerType TriggerType { get; set; }

        /// <summary>
        /// 执行间隔时间, 秒为单位
        /// </summary>
        public int? IntervalSecond { get; set; }

        /// <summary>
        /// 循环执行次数
        /// </summary>
        public int? CycleRunTimes { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 执行传参
        /// </summary>
        public string? RunParams { get; set; }

        /// <summary>
        /// 触发器状态
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public TriggerState TriggerState { get; set; }
    }
}
