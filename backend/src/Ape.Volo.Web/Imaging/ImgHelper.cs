using System.Text.RegularExpressions;
using SkiaSharp;

namespace Ape.Volo.Web.Imaging;

/// <summary>
/// 图片操作帮助类
/// </summary>
/// <remarks>
/// 全部基于 SkiaSharp，Windows / Linux / macOS 通用。
/// 原先用 System.Drawing，在非 Windows 平台会抛 PlatformNotSupportedException。
/// 返回的 <see cref="SKBitmap" /> 由调用方负责 Dispose。
/// </remarks>
public static class ImgHelper
{
    /// <summary>
    /// 默认 JPEG 编码质量
    /// </summary>
    private const int DefaultQuality = 90;

    /// <summary>
    /// 从文件获取图片
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <returns></returns>
    public static SKBitmap GetImgFromFile(string fileName)
    {
        using var stream = File.OpenRead(fileName);

        return SKBitmap.Decode(stream) ?? throw new InvalidOperationException($"无法解析图片文件：{fileName}");
    }

    /// <summary>
    /// 从base64字符串读入图片
    /// </summary>
    /// <param name="base64">base64字符串</param>
    /// <returns></returns>
    public static SKBitmap GetImgFromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);

        return SKBitmap.Decode(bytes) ?? throw new InvalidOperationException("无法解析 base64 图片数据");
    }

    /// <summary>
    /// 从URL格式的Base64图片获取真正的图片
    /// 即去掉data:image/jpg;base64,这样的格式
    /// </summary>
    /// <param name="base64Url">图片Base64的URL形式</param>
    /// <returns></returns>
    public static SKBitmap GetImgFromBase64Url(string base64Url)
    {
        var base64 = GetBase64String(base64Url);

        return GetImgFromBase64(base64);
    }

    /// <summary>
    /// 压缩图片
    /// 注:等比压缩
    /// </summary>
    /// <param name="img">原图片</param>
    /// <param name="width">压缩后宽度</param>
    /// <returns></returns>
    public static SKBitmap CompressImg(SKBitmap img, int width)
    {
        if (img.Width <= 0)
        {
            throw new ArgumentException("原图片宽度无效", nameof(img));
        }

        var height = (int)Math.Round((double)width / img.Width * img.Height);

        return CompressImg(img, width, height);
    }

    /// <summary>
    /// 压缩图片
    /// </summary>
    /// <param name="img">原图片</param>
    /// <param name="width">压缩后宽度</param>
    /// <param name="height">压缩后高度</param>
    /// <returns></returns>
    public static SKBitmap CompressImg(SKBitmap img, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "压缩后的宽高必须大于 0");
        }

        var info = new SKImageInfo(width, height, img.ColorType, img.AlphaType);
        var target = new SKBitmap(info);

        using var canvas = new SKCanvas(target);
        using var source = SKImage.FromBitmap(img);

        canvas.Clear(SKColors.Transparent);
        canvas.DrawImage(source, SKRect.Create(width, height),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        canvas.Flush();

        return target;
    }

    /// <summary>
    /// 将图片转为base64字符串
    /// 默认使用jpg格式
    /// </summary>
    /// <param name="img">图片对象</param>
    /// <returns></returns>
    public static string ToBase64String(SKBitmap img)
    {
        return ToBase64String(img, SKEncodedImageFormat.Jpeg, DefaultQuality);
    }

    /// <summary>
    /// 将图片转为base64字符串
    /// 使用指定格式
    /// </summary>
    /// <param name="img">图片对象</param>
    /// <param name="imageFormat">指定格式</param>
    /// <param name="quality">编码质量，仅对有损格式生效</param>
    /// <returns></returns>
    public static string ToBase64String(SKBitmap img, SKEncodedImageFormat imageFormat, int quality = DefaultQuality)
    {
        using var image = SKImage.FromBitmap(img);
        using var data = image.Encode(imageFormat, quality);

        return Convert.ToBase64String(data.ToArray());
    }

    /// <summary>
    /// 将字节数组转为带前缀的 base64 字符串
    /// </summary>
    /// <param name="bytes">图片字节</param>
    /// <returns></returns>
    public static string ToBase64StringUrl(byte[] bytes)
    {
        return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 将图片转为base64字符串
    /// 默认使用jpg格式,并添加data:image/jpeg;base64,前缀
    /// </summary>
    /// <param name="img">图片对象</param>
    /// <returns></returns>
    public static string ToBase64StringUrl(SKBitmap img)
    {
        return ToBase64StringUrl(img, SKEncodedImageFormat.Jpeg, DefaultQuality);
    }

    /// <summary>
    /// 将图片转为base64字符串
    /// 使用指定格式,并添加data:image/{格式};base64,前缀
    /// </summary>
    /// <param name="img">图片对象</param>
    /// <param name="imageFormat">指定格式</param>
    /// <param name="quality">编码质量，仅对有损格式生效</param>
    /// <returns></returns>
    public static string ToBase64StringUrl(SKBitmap img, SKEncodedImageFormat imageFormat, int quality = DefaultQuality)
    {
        var base64 = ToBase64String(img, imageFormat, quality);
        var mime = imageFormat.ToString().ToLowerInvariant();

        return $"data:image/{mime};base64,{base64}";
    }

    /// <summary>
    /// 获取真正的图片base64数据
    /// 即去掉data:image/jpg;base64,这样的格式
    /// </summary>
    /// <param name="base64UrlStr">带前缀的base64图片字符串</param>
    /// <returns></returns>
    public static string GetBase64String(string base64UrlStr)
    {
        var parttern = "^(data:image/.*?;base64,).*?$";

        var match = Regex.Match(base64UrlStr, parttern);
        if (match.Groups.Count > 1)
            base64UrlStr = base64UrlStr.Replace(match.Groups[1].ToString(), "");

        return base64UrlStr;
    }
}
