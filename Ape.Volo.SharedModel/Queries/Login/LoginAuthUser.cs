using System.ComponentModel.DataAnnotations;

namespace Ape.Volo.SharedModel.Queries.Login;

/// <summary>
/// 登录用户
/// </summary>
public class LoginAuthUser
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Display(Name = "User.Username")]
    [Required(ErrorMessage = "{0}required")]
    public string UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Display(Name = "User.Password")]
    [Required(ErrorMessage = "{0}required")]
    public string Password { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    [Display(Name = "Sys.Captcha")]
    public string Captcha { get; set; }

    /// <summary>
    /// 验证码ID
    /// </summary>
    [Display(Name = "Sys.CaptchaId")]
    public string CaptchaId { get; set; }
}
