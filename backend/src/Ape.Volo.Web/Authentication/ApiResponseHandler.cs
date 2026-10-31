using System.Text.Encodings.Web;
using Ape.Volo.Common.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 认证失败处理器，把 401/403 统一包装为系统标准响应体
/// </summary>
/// <param name="options">认证方案选项</param>
/// <param name="logger">日志工厂</param>
/// <param name="encoder">URL 编码器</param>
public class ApiResponseHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>
    /// 不做任何认证处理，认证由 JwtBearer 完成
    /// </summary>
    /// <returns>始终返回无结果的认证结果</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
        Task.FromResult(AuthenticateResult.NoResult());

    /// <summary>
    /// 未携带有效凭证时返回 401 及提示信息
    /// </summary>
    /// <param name="properties">认证属性</param>
    /// <returns>异步任务</returns>
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = "Bearer";
        return WriteErrorAsync(StatusCodes.Status401Unauthorized, "登录凭证无效或会话已过期，请重新登录。");
    }

    /// <summary>
    /// 已认证但无权限时返回 403 及提示信息
    /// </summary>
    /// <param name="properties">认证属性</param>
    /// <returns>异步任务</returns>
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties) =>
        WriteErrorAsync(StatusCodes.Status403Forbidden, "抱歉，您访问权限等级不够。");

    private Task WriteErrorAsync(int status, string message)
    {
        Response.ContentType = "application/json";
        Response.StatusCode = status;
        return Response.WriteAsync(new ActionResultVm
        {
            Status = status,
            ActionError = new ActionError(),
            Message = message,
            Path = Request.Path.Value!
        }.ToJson());
    }
}
