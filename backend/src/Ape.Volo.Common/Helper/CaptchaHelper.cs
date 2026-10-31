namespace Ape.Volo.Common.Helper;

/// <summary>
/// 纯文本验证码生成（不涉及图像，任何宿主都可使用）
/// </summary>
public static class CaptchaHelper
{
    /// <summary>
    /// 构造邮件验证码
    /// </summary>
    /// <param name="length">验证码长度</param>
    /// <returns>纯数字验证码</returns>
    public static string BuildEmailCaptcha(int length)
    {
        return CreateValidateCode(length);
    }

    /// <summary>
    /// 生成指定长度的数字验证码
    /// </summary>
    /// <param name="length">验证码长度</param>
    /// <returns>纯数字验证码</returns>
    public static string CreateValidateCode(int length)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        return string.Create(length, Random.Shared, static (buffer, random) =>
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (char)('0' + random.Next(10));
            }
        });
    }
}
