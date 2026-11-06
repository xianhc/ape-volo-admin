using Ape.Volo.Common.Attributes;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 验证码配置
/// </summary>
[OptionsSettings]
public class CaptchaOptions
{
    /// <summary>
    /// 验证码长度
    /// </summary>
    public int CodeLength { get; set; }

    /// <summary>
    /// 图像宽度
    /// </summary>
    public int ImageWidth { get; set; }

    /// <summary>
    /// 图像高度
    /// </summary>
    public int ImageHeight { get; set; }

    /// <summary>
    /// 字体大小
    /// </summary>
    public int FontSize { get; set; }

    /// <summary>
    /// 触发验证码的失败次数阈值(超出则显示验证码)
    /// </summary>
    public int FailureThreshold { get; set; }

    /// <summary>
    /// 失败计数器过期时间 单位：s(秒)
    /// </summary>
    public int CounterTtlSeconds { get; set; }
}
