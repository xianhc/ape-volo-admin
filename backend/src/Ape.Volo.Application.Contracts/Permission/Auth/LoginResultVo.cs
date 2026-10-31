using Ape.Volo.Application.Contracts.Permission.Users.Models;
using Newtonsoft.Json;

namespace Ape.Volo.Application.Contracts.Permission.Auth;

/// <summary>
/// 登录结果
/// </summary>
public class LoginResultVo
{
    // [JsonProperty("user")]
    // public JwtUserVo JwtUserVo { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    public UserVo User { get; set; } = default!;

    /// <summary>
    /// 角色权限
    /// </summary>
    public List<string> RoleCodes { get; set; } = new();

    /// <summary>
    /// 按钮权限
    /// </summary>
    public List<string> AuthCodes { get; set; } = new();


    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("token")]
    public TokenVo TokenVo { get; set; } = default!;
}