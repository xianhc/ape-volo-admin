

namespace Ape.Volo.Web.Models;

/// <summary>
/// 错误
/// </summary>
public class ActionError
{
    /// <summary>
    /// 错误集合
    /// </summary>
    public Dictionary<string, string> Errors { get; set; } = new();

    /// <summary>
    /// 第一个错误
    /// </summary>
    /// <returns></returns>
    public string GetFirstError()
    {
        return Errors?.Values.FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
               ?? "请求参数校验失败。";
    }
}
