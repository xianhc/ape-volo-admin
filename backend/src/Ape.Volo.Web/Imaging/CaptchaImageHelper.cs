using Ape.Volo.Common.Helper;
using SkiaSharp;

namespace Ape.Volo.Web.Imaging;

/// <summary>
/// 验证码图像辅助工具（基于 SkiaSharp 自绘）
/// </summary>
public static class CaptchaImageHelper
{
    #region Fields

    /// <summary>
    /// 首选字体名称
    /// </summary>
    private const string PreferredFontName = "Arial";

    /// <summary>
    /// 字符绘制可选颜色
    /// </summary>
    private static readonly SKColor[] Colors =
    {
        SKColors.Black,
        SKColors.Red,
        SKColors.Blue,
        SKColors.Green,
        SKColors.Orange,
        SKColors.Brown,
        SKColors.DarkBlue
    };

    #endregion

    #region 验证码生成

    /// <summary>
    /// 配置未给出有效长度时使用的默认验证码长度
    /// </summary>
    private const int DefaultLength = 4;

    /// <summary>
    /// 生成数字验证码及其 JPEG 图片
    /// </summary>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="fontSize">字体大小</param>
    /// <param name="length">验证码长度，取自 <c>Captcha:KeyLength</c>；小于等于 0 时使用默认长度</param>
    /// <returns>图片字节与验证码答案</returns>
    public static (byte[] imgBytes, string code) BuildVerifyCode(int width, int height, int fontSize,
        int length = 0)
    {
        var code = CaptchaHelper.CreateValidateCode(length > 0 ? length : DefaultLength);
        return (DrawCaptchaCore(code, width, height, fontSize), code);
    }

    #endregion

    #region 图像绘制

    /// <summary>
    /// 根据验证码内容完成图形绘制
    /// </summary>
    /// <param name="expression">待绘制的验证码文本</param>
    /// <param name="width">图像宽度</param>
    /// <param name="height">图像高度</param>
    /// <param name="fontSize">字体大小</param>
    /// <returns>JPEG 图片字节</returns>
    private static byte[] DrawCaptchaCore(string expression, int width, int height, int fontSize)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "图片宽度必须大于 0。");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "图片高度必须大于 0。");
        }

        if (fontSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fontSize), "字体大小必须大于 0。");
        }

        var random = Random.Shared;
        var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var bitmap = new SKBitmap(imageInfo);
        using var canvas = new SKCanvas(bitmap);
        using var typeface = CreateTypeface();
        using var font = new SKFont(typeface, fontSize);
        using var paint = new SKPaint();
        paint.IsAntialias = true;

        // 背景色
        canvas.Clear(SKColors.White);

        // 画验证码：按可用宽度均分字符间距，并做轻微纵向抖动
        var charStep = Math.Max(10f, (width - 10f) / Math.Max(expression.Length, 1));
        var metrics = font.Metrics;
        var centeredBaseline = (height - metrics.Ascent - metrics.Descent) / 2f;
        var jitterRange = Math.Max(1, Math.Min(4, height / 10));

        for (var i = 0; i < expression.Length; i++)
        {
            paint.Color = RandomColor(random);
            paint.Style = SKPaintStyle.Fill;
            var baseline = centeredBaseline + random.Next(-jitterRange, jitterRange + 1);
            canvas.DrawText(expression[i].ToString(), 5f + i * charStep, baseline, font, paint);
        }

        // 画干扰线
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1f;
        for (var i = 0; i < 6; i++)
        {
            paint.Color = RandomColor(random);
            canvas.DrawLine(
                random.Next(width),
                random.Next(height),
                random.Next(width),
                random.Next(height),
                paint);
        }

        // 画噪点
        paint.Style = SKPaintStyle.Fill;
        for (var i = 0; i < 60; i++)
        {
            paint.Color = RandomColor(random);
            canvas.DrawPoint(random.Next(width), random.Next(height), paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }

    /// <summary>
    /// 创建字体
    /// </summary>
    /// <returns>可用的粗体字体</returns>
    private static SKTypeface CreateTypeface()
    {
        return SKTypeface.FromFamilyName(PreferredFontName, SKFontStyle.Bold)
               ?? SKTypeface.FromFamilyName(null, SKFontStyle.Bold)
               ?? throw new InvalidOperationException("当前系统没有可用于生成验证码的字体。");
    }

    /// <summary>
    /// 在允许的范围内生成随机绘制颜色
    /// </summary>
    /// <param name="random">随机数发生器</param>
    /// <returns>绘制颜色</returns>
    private static SKColor RandomColor(Random random)
    {
        return Colors[random.Next(Colors.Length)];
    }

    #endregion
}
