using Ape.Volo.Shared.WebApp;

namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 令牌签发端口：应用层编排登录时只依赖该抽象，JWT 细节留在宿主层。
/// </summary>
public interface ITokenIssuer
{
    /// <summary>
    /// 为已通过校验的用户签发访问令牌与刷新令牌。
    /// </summary>
    /// <param name="loginUserInfo">登录用户信息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>令牌对；会话创建失败时返回 null</returns>
    Task<TokenVo?> IssueAsync(LoginUserInfo loginUserInfo, CancellationToken ct = default);
}
