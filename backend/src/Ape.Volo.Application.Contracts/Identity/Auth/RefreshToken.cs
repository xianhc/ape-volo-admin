using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// 登录或上次刷新返回的 refreshToken；访问令牌不能用于刷新
    /// </summary>
    [Required(ErrorMessage = "{0}不能为空。")]
    public string Token { get; set; } = string.Empty;

}
