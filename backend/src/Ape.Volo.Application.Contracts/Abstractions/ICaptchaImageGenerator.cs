namespace Ape.Volo.Application.Contracts.Abstractions;

/// <summary>
/// 验证码图片生成端口：应用层只依赖该抽象，绘图库与字体资源留在宿主层。
/// </summary>
public interface ICaptchaImageGenerator
{
    /// <summary>
    /// 生成验证码图片及其答案。
    /// </summary>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="keyLength">验证码长度；为 0 时生成算术验证码</param>
    /// <returns>可直接放进 img 标签的 data URL 与验证码答案</returns>
    CaptchaImage Create(int width, int height, int fontSize, int keyLength);
}

/// <summary>
/// 验证码图片与答案。
/// </summary>
/// <param name="ImageDataUrl">data URL 形式的图片</param>
/// <param name="Code">验证码答案</param>
public sealed record CaptchaImage(string ImageDataUrl, string Code);
