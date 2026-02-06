using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.Message.Email
{
    /// <summary>
    /// 邮件模板
    /// </summary>
    [SugarTable("email_message_template")]
    public class EmailMessageTemplate : BaseEntity
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 抄送邮箱地址
        /// </summary>
        public string? BccEmailAddresses { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 邮箱账户标识符
        /// </summary>
        public long EmailAccountId { get; set; }
    }
}
