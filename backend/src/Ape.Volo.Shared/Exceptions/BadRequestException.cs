namespace Ape.Volo.Shared.Exceptions;

/// <summary>
/// 请求错误。业务层用它表达「输入不合法」，由宿主层转换为对应的 HTTP 响应。
/// </summary>
public class BadRequestException : System.Exception
{
    /// <summary>
    /// HTTP 状态码，默认 400。刻意不用 StatusCodes 常量：契约层不依赖宿主框架。
    /// </summary>
    public int StatusCode { get; set; } = 400;

    /// <summary>
    /// 字段级错误明细
    /// </summary>
    public Dictionary<string, string> Errors { get; set; } = new();

    /// <summary>
    /// 初始化 <see cref="BadRequestException"/> 的新实例。
    /// </summary>
    /// <param name="message">错误信息</param>
    /// <param name="errors">字段级错误明细</param>
    public BadRequestException(string message, Dictionary<string, string>? errors = null) : base(message)
    {
    }
}
