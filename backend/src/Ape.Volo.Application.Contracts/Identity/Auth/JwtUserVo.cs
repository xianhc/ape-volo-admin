using Ape.Volo.Application.Contracts.Identity.Users.Models;

namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// JWT令牌用户
/// </summary>
public class JwtUserVo
{
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
}