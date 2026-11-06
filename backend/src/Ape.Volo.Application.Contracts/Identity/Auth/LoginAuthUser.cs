using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 登录用户
/// </summary>
public class LoginAuthUser
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "用户名称")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [StringLength(64)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 密码（4096 位 RSA-OAEP-SHA256 密文的标准 Base64 串，固定 684 字符）
    /// </summary>
    [Display(Name = "用户密码")]
    [Required(ErrorMessage = "{0}不能为空。")]
    [StringLength(684, MinimumLength = 684)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 验证码
    /// </summary>
    [Display(Name = "验证码")]
    [StringLength(64)]
    public string Captcha { get; set; } = string.Empty;

    /// <summary>
    /// 验证码ID
    /// </summary>
    [Display(Name = "验证码Id")]
    [StringLength(128)]
    public string CaptchaId { get; set; } = string.Empty;
}
