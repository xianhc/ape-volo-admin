namespace Ape.Volo.Web.Imaging;

/// <summary>
/// 基于 SkiaSharp 的验证码图片生成：绘图与编码留在宿主层，应用层只拿 data URL。
/// </summary>
public sealed class CaptchaImageGenerator : ICaptchaImageGenerator
{
    /// <inheritdoc />
    public CaptchaImage Create(int width, int height, int fontSize, int keyLength)
    {
        var (imgBytes, code) = CaptchaImageHelper.BuildVerifyCode(width, height, fontSize, keyLength);
        return new CaptchaImage(ImgHelper.ToBase64StringUrl(imgBytes), code);
    }
}
