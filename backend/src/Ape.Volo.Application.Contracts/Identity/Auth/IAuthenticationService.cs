namespace Ape.Volo.Application.Contracts.Identity.Auth;

/// <summary>
/// 登录用例：验证码阈值、失败计数、账号锁定、凭据校验与令牌签发编排都在应用层完成。
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// 生成验证码并写入缓存，同时返回当前是否需要展示验证码。
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>验证码图片、标识与展示开关</returns>
    Task<CaptchaVo> CreateCaptchaAsync(CancellationToken ct = default);

    /// <summary>
    /// 校验凭据并完成登录；失败信息通过返回值表达。
    /// </summary>
    /// <param name="authUser">登录请求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>登录结果</returns>
    Task<LoginResult> LoginAsync(LoginAuthUser authUser, CancellationToken ct = default);

    /// <summary>
    /// 组装当前登录用户的个人信息、角色与权限码。
    /// </summary>
    /// <param name="userId">用户 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>当前用户信息</returns>
    Task<JwtUserVo> GetCurrentUserInfoAsync(long userId, CancellationToken ct = default);
}
