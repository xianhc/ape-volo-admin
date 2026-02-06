using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.System
{
    /// <summary>
    /// 三方应用密钥
    /// </summary>
    [SugarTable("sys_app_secret")]
    public class AppSecret : BaseEntity
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        public string AppId { get; set; } = string.Empty;

        /// <summary>
        /// 应用秘钥
        /// </summary>
        public string AppSecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
