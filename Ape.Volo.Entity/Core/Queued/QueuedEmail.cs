using System;
using Ape.Volo.Common.Enums;
using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.Queued
{
    /// <summary>
    /// 邮件队列
    /// </summary>
    [SugarTable("queued_email")]
    public class QueuedEmail : BaseEntity
    {
        /// <summary>
        /// 发件邮箱
        /// </summary>
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// 发件人名称
        /// </summary>
        public string? FromName { get; set; }

        /// <summary>
        /// 收件邮箱
        /// </summary>
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// 收件人名称
        /// </summary>
        public string? ToName { get; set; }

        /// <summary>
        /// 回复邮箱
        /// </summary>
        public string? ReplyTo { get; set; }

        /// <summary>
        /// 回复人名称
        /// </summary>
        public string? ReplyToName { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public QueuedEmailPriority Priority { get; set; }

        /// <summary>
        /// 抄送
        /// </summary>
        public string? Cc { get; set; }

        /// <summary>
        /// 密件抄送
        /// </summary>
        public string? Bcc { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// 发送上限次数
        /// </summary>
        public int SentTries { get; set; }

        /// <summary>
        /// 是否已发送
        /// </summary>
        public bool IsSend { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime? SendTime { get; set; }

        /// <summary>
        /// 发件邮箱ID
        /// </summary>
        public long EmailAccountId { get; set; }
    }
}
