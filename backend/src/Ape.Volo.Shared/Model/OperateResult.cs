

namespace Ape.Volo.Shared.Model;

/// <summary>
/// 操作结果
/// </summary>
public class OperateResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// 结果
    /// </summary>
    /// <returns></returns>
    public static OperateResult Result(bool result) => new() { IsSuccess = result };

    /// <summary>
    /// 成功
    /// </summary>
    /// <returns></returns>
    public static OperateResult Success(string message = "") => new() { IsSuccess = true, Message = message };

    /// <summary>
    /// 错误
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static OperateResult Error(string message) => new() { IsSuccess = false, Message = message };
}
