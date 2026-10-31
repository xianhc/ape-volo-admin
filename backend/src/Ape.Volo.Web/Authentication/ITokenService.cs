using System.Security.Claims;
using Ape.Volo.Shared.WebApp;

namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 令牌服务：签发、刷新、校验与撤销访问令牌
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 为登录用户创建服务端会话并签发访问令牌与刷新令牌
    /// </summary>
    /// <param name="loginUserInfo">登录用户信息</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>令牌对；会话创建失败时返回 null</returns>
    Task<TokenVo?> IssueTokenAsync(LoginUserInfo loginUserInfo, CancellationToken ct = default);

    /// <summary>
    /// 使用刷新令牌换取新的令牌对，旧刷新令牌随即失效
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新的令牌对；令牌无效、已使用或会话已撤销时返回 null</returns>
    Task<TokenVo?> RefreshAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// 校验访问令牌是否仍对应当前有效的服务端会话
    /// </summary>
    /// <param name="principal">访问令牌解析出的用户主体</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>会话有效返回 true</returns>
    Task<bool> ValidateAccessAsync(ClaimsPrincipal principal, CancellationToken ct = default);

    /// <summary>
    /// 撤销令牌所属会话，访问令牌与刷新令牌均可传入
    /// </summary>
    /// <param name="token">待撤销的令牌</param>
    /// <param name="ct">取消令牌</param>
    Task RevokeAsync(string token, CancellationToken ct = default);
}
