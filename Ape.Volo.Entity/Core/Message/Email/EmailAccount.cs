using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.Message.Email
{
    /// <summary>
    /// 邮件账户
    /// </summary>
    [SugarTable("email_account")]
    [SugarIndex("unique_{table}_Email", nameof(Email), OrderByType.Asc, true)]
    public class EmailAccount : BaseEntity
    {
        /// <summary>
        ///电子邮件地址
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 电子邮件显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 电子邮件主机
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// 电子邮件端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 电子邮件用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 电子邮件密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 是否SSL
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 是否与请求一起发送应用程序的默认系统凭据
        /// </summary>
        public bool UseDefaultCredentials { get; set; }
    }
}