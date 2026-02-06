using Ape.Volo.Entity.Base;
using SqlSugar;

namespace Ape.Volo.Entity.Core.System
{
    /// <summary>
    /// Token黑名单
    /// </summary>
    [SugarTable("sys_token_blacklist")]
    public class TokenBlacklist : BaseEntity
    {
        /// <summary>
        /// 令牌 登录token的MD5值
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
    }
}
