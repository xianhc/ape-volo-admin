namespace Ape.Volo.Web.Authentication;

/// <summary>
/// Web 认证协议和声明约定，由宿主负责解释。
/// </summary>
public static class AuthConstants
{
    /// <summary>
    /// HTTP 认证凭证类型。
    /// </summary>
    public const string JwtTokenType = "Bearer";

    /// <summary>
    /// 服务端签发并由 HTTP 身份适配器读取的声明名。
    /// </summary>
    public static class JwtClaimTypes
    {
        /// <summary>
        /// 用户 ID。
        /// </summary>
        public const string Sub = "sub";
        /// <summary>
        /// 账号。
        /// </summary>
        public const string Name = "name";
        /// <summary>
        /// 部门 ID。
        /// </summary>
        public const string DeptId = "dept_id";
    }
}
