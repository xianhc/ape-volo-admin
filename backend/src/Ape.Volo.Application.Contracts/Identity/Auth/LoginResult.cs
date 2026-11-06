namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 登录结果。业务失败通过返回值表达，不抛异常 —— 抛异常会进入异常日志管道，
/// 每次密码输错都会落一条记录。
/// </summary>
public sealed class LoginResult
{
    /// <summary>
    /// 是否登录成功
    /// </summary>
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// 失败时应返回的 HTTP 状态码，默认 400
    /// </summary>
    public int StatusCode { get; private init; } = 400;

    /// <summary>
    /// 失败提示，成功时为空
    /// </summary>
    public string Message { get; private init; } = string.Empty;

    /// <summary>
    /// 成功时的登录结果
    /// </summary>
    public LoginResultVo? Data { get; private init; }

    /// <summary>
    /// 登录成功。
    /// </summary>
    /// <param name="data">登录结果</param>
    /// <returns>成功结果</returns>
    public static LoginResult Success(LoginResultVo data) =>
        new() { IsSuccess = true, StatusCode = 200, Data = data };

    /// <summary>
    /// 业务校验失败（400）。
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <returns>失败结果</returns>
    public static LoginResult Failure(string message) => new() { Message = message };

    /// <summary>
    /// 凭据或状态已失效（401），无提示时返回空响应体。
    /// </summary>
    /// <param name="message">提示信息</param>
    /// <returns>未授权结果</returns>
    public static LoginResult Unauthorized(string message = "") =>
        new() { StatusCode = 401, Message = message };
}
