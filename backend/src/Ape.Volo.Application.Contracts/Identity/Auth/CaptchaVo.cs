

namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 验证码
/// </summary>
public class CaptchaVo
{
    /// <summary>
    /// 图片base64
    /// </summary>
    public string Img { get; set; } = string.Empty;

    /// <summary>
    /// 验证码ID
    /// </summary>
    public string CaptchaId { get; set; } = string.Empty;

    /// <summary>
    /// 是否显示验证码
    /// </summary>
    public bool ShowCaptcha { get; set; }
}
