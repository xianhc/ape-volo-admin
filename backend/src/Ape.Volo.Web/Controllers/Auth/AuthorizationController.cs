using System.ComponentModel;
using Ape.Volo.Web.Controllers.Base;
using Ape.Volo.Application.Contracts.Identity.Users.Models;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Web.Controllers.Auth;

/// <summary>
/// 授权管理
/// </summary>
[Area("授权管理")]
[Route("/api/auth")]
public class AuthorizationController : BaseApiController
{
    #region 构造函数

    /// <summary>
    /// 初始化 <see cref="AuthorizationController"/> 的新实例。
    /// </summary>
    /// <param name="authenticationService">登录与验证码用例</param>
    /// <param name="tokenService">令牌刷新与撤销</param>
    /// <param name="queuedEmailService">邮件验证码</param>
    /// <param name="currentUser">当前请求用户</param>
    public AuthorizationController(IAuthenticationService authenticationService, ITokenService tokenService,
        IQueuedEmailService queuedEmailService, ICurrentUser currentUser)
    {
        _authenticationService = authenticationService;
        _tokenService = tokenService;
        _queuedEmailService = queuedEmailService;
        _currentUser = currentUser;
    }

    #endregion

    #region 字段

    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly ICurrentUser _currentUser;

    #endregion

    #region 内部接口

    /// <summary>
    /// 获取验证码
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [HttpGet]
    [Description("获取验证码")]
    [Route("captcha")]
    [AllowAnonymous]
    [NotOperate]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CaptchaVo))]
    public async Task<ActionResult> Captcha(CancellationToken ct = default)
        => JsonContent(await _authenticationService.CreateCaptchaAsync(ct));


    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="authUser"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [HttpPost]
    [Route("login")]
    [Description("用户登录")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResultVo))]
    public async Task<ActionResult> Login([FromBody] LoginAuthUser authUser, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _authenticationService.LoginAsync(authUser, ct);
        if (result.IsSuccess)
        {
            return JsonContent(result.Data!);
        }

        // 只有「凭据或状态已失效」用 401，其余业务校验失败保持 400
        if (result.StatusCode != StatusCodes.Status401Unauthorized)
        {
            return Error(result.Message);
        }

        return result.Message.IsNullOrEmpty()
            ? Unauthorized()
            : Unauthorized(new ActionResultVm { Status = result.StatusCode, Message = result.Message });
    }


    /// <summary>
    /// 刷新Token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [HttpPost]
    [Route("refresh-token")]
    [Description("刷新Token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenVo))]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshToken refreshToken, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var pair = await _tokenService.RefreshAsync(refreshToken?.Token!, ct);
        if (pair == null)
            return Unauthorized(new ActionResultVm
            {
                Status = StatusCodes.Status401Unauthorized,
                Message = "刷新凭证无效、已使用或会话已撤销，请重新登录。"
            });

        return JsonContent(pair);
    }


    /// <summary>
    /// 获取当前登录用户的个人信息、角色与权限码
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>用户信息及授权码集合</returns>
    [HttpGet]
    [Route("info")]
    [Description("个人信息")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JwtUserVo))]
    public async Task<ActionResult> GetInfo(CancellationToken ct = default)
    {
        if (_currentUser.IsNull() || _currentUser.Id == 0)
        {
            return Error("Token已过期，请重新登录！");
        }

        return JsonContent(await _authenticationService.GetCurrentUserInfoAsync(_currentUser.Id, ct));
    }


    /// <summary>
    /// 获取邮箱验证码，申请变更邮箱
    /// </summary>
    /// <param name="emailCodeDto"></param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [HttpPost]
    [Description("获取邮箱验证码")]
    [Route("reset-email-code")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> ResetEmailCode([FromBody] EmailCodeDto emailCodeDto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _queuedEmailService.ResetEmailCode(emailCodeDto.Email, "EmailVerificationCode", ct);
        return Ok(result);
    }


    /// <summary>
    /// 系统用户登出
    /// </summary>
    /// <param name="request">刷新凭证，可用于访问令牌过期后的退出</param>
    /// <param name="ct">取消令牌</param>
    /// <returns></returns>
    [HttpDelete]
    [Route("logout")]
    [Description("用户登出")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Logout(
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)]
        RefreshToken? request = null, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request?.Token))
            await _tokenService.RevokeAsync(request.Token, ct);

        var authorization = Request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            await _tokenService.RevokeAsync(authorization[7..].Trim(), ct);

        return Ok(OperateResult.Success());
    }

    #endregion
}
