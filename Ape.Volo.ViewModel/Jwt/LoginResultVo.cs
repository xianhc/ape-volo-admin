using Ape.Volo.ViewModel.Core.Permission.User;
using Newtonsoft.Json;

namespace Ape.Volo.ViewModel.Jwt;

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
    public UserVo User { get; set; }

    /// <summary>
    /// 角色权限
    /// </summary>
    public List<string> RoleCodes { get; set; }

    /// <summary>
    /// 按钮权限
    /// </summary>
    public List<string> AuthCodes { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("token")]
    public TokenVo TokenVo { get; set; }
}