using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Api.Controllers.Base;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Common.Model;
using Ape.Volo.Common.WebApp;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Utils;
using Ape.Volo.IBusiness.Permission;
using Ape.Volo.IBusiness.Queued;
using Ape.Volo.IBusiness.System;
using Ape.Volo.Infrastructure.Authentication;
using Ape.Volo.SharedModel.Dto.Core.Permission.User;
using Ape.Volo.SharedModel.Dto.Jwt;
using Ape.Volo.SharedModel.Queries.Login;
using Ape.Volo.ViewModel.Core.Permission.User;
using Ape.Volo.ViewModel.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ape.Volo.Api.Controllers.Auth;

/// <summary>
/// 授权管理
/// </summary>
[Area("Area.AuthorizationManagement")]
[Route("/auth")]
public class AuthorizationController : BaseApiController
{
    #region 构造函数

    public AuthorizationController(IUserService userService, IPermissionService permissionService,
        IOnlineUserService onlineUserService, IQueuedEmailService queuedEmailService,
        ITokenService tokenService, ITokenBlacklistService tokenBlacklistService)
    {
        _userService = userService;
        _permissionService = permissionService;
        _onlineUserService = onlineUserService;
        _queuedEmailService = queuedEmailService;
        _tokenService = tokenService;
        _tokenBlacklistService = tokenBlacklistService;
    }

    #endregion

    #region 字段

    private readonly IUserService _userService;
    private readonly IPermissionService _permissionService;
    private readonly IOnlineUserService _onlineUserService;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly ITokenService _tokenService;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    #endregion

    #region 内部接口

    /// <summary>
    /// 获取验证码
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Description("Action.GetVerificationCode")]
    [Route("captcha")]
    [AllowAnonymous]
    [NotOperate]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CaptchaVo))]
    public async Task<ActionResult> Captcha()
    {
        var showCaptcha = true; //是否显示验证码
        var captchaOptions = App.GetOptions<CaptchaOptions>();
        if (captchaOptions.Threshold > 0)
        {
            var thresholdCacheKey =
                GlobalConstants.CachePrefix.Threshold + App.HttpContext.Connection.RemoteIpAddress;
            var failedThreshold = await App.Cache.GetAsync<int>(thresholdCacheKey);
            if (failedThreshold <= 0)
            {
                failedThreshold = 1;
                await App.Cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut), null);
            }

            showCaptcha = failedThreshold > captchaOptions.Threshold;
        }


        var (imgBytes, code) = SixLaborsImageHelper.BuildVerifyCode(captchaOptions.ImgWidth,
            captchaOptions.ImgHeight,
            captchaOptions.FontSize, captchaOptions.KeyLength);
        var img = ImgHelper.ToBase64StringUrl(imgBytes);
        var captchaId = GlobalConstants.CachePrefix.CaptchaId +
                        IdHelper.NextId().ToString().Base64Encode();
        await App.Cache.SetAsync(captchaId, code, TimeSpan.FromMinutes(2), null);
        var captchaVo = new CaptchaVo { Img = img, CaptchaId = captchaId, ShowCaptcha = showCaptcha };

        return JsonContent(captchaVo);
    }


    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="authUser"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("login")]
    [Description("Action.UserLogin")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResultVo))]
    public async Task<ActionResult> Login([FromBody] LoginAuthUser authUser)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }


        var loginFailedLimitOptions = App.GetOptions<LoginFailedLimitOptions>();
        var attempsCacheKey = GlobalConstants.CachePrefix.Attempts + App.HttpContext.Connection.RemoteIpAddress +
                              authUser.UserName;
        LoginAttempt loginAttempt = null;
        if (loginFailedLimitOptions.Enabled)
        {
            loginAttempt = await App.Cache.GetAsync<LoginAttempt>(attempsCacheKey);
            if (loginAttempt.IsNull())
            {
                loginAttempt = new LoginAttempt { Count = 0, IsLocked = false, LockUntil = DateTime.MinValue };
                await App.Cache.SetAsync(attempsCacheKey, loginAttempt,
                    TimeSpan.FromSeconds(loginFailedLimitOptions.Lockout), null);
            }

            if (loginAttempt.IsLocked && DateTime.Now < loginAttempt.LockUntil)
            {
                // 可以实施账户锁定时，通过邮件或短信通知用户。
                // 可以实施账户锁定后要求管理员手动解锁
                return Error(App.L.R("Error.AccountLockedWithUnlockTime{0}",
                    loginAttempt.LockUntil.ToString("yyyy-MM-dd HH:mm:ss")));
            }
        }


        var captchaOptions = App.GetOptions<CaptchaOptions>();
        var showCaptcha = true; //是否显示验证码
        var thresholdCacheKey = GlobalConstants.CachePrefix.Threshold + App.HttpContext.Connection.RemoteIpAddress;
        var failedThreshold = 0;
        if (captchaOptions.Threshold > 0)
        {
            failedThreshold = await App.Cache.GetAsync<int>(thresholdCacheKey);
            if (failedThreshold <= 0)
            {
                failedThreshold = 1;
                await App.Cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut), null);
            }

            showCaptcha = failedThreshold > captchaOptions.Threshold;
        }


        if (App.GetOptions<SystemOptions>().RunMode != RunMode.Dev && showCaptcha)
        {
            if (authUser.Captcha.IsNullOrEmpty())
            {
                return Error(ValidationError.Required(authUser, nameof(authUser.Captcha)));
            }

            if (authUser.CaptchaId.IsNullOrEmpty())
            {
                return Error(ValidationError.Required(authUser, nameof(authUser.CaptchaId)));
            }


            var code = await App.Cache.GetAsync<string>(authUser.CaptchaId);
            if (code.IsNullOrEmpty())
            {
                return Error(App.L.R("Error.VerificationCodeExpired"));
            }

            if (!code.Equals(authUser.Captcha))
            {
                if (captchaOptions.Threshold > 0)
                {
                    failedThreshold++;
                    await App.Cache.SetAsync(thresholdCacheKey, failedThreshold,
                        TimeSpan.FromSeconds(captchaOptions.TimeOut),
                        null);
                }

                return Error(App.L.R("Error.InvalidVerificationCode"));
            }
        }

        var userDto = await _userService.QueryByNameAsync(authUser.UserName);
        if (userDto == null)
        {
            if (captchaOptions.Threshold > 0)
            {
                failedThreshold++;
                await App.Cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut),
                    null);
            }

            return Error(App.L.R("Error.UserNotFound"));
        }

        var rsaOptions = App.GetOptions<RsaOptions>();
        var password = new RsaHelper(rsaOptions.PrivateKey, rsaOptions.PublicKey).Decrypt(authUser.Password);
        if (!BCryptHelper.Verify(password, userDto.Password))
        {
            if (captchaOptions.Threshold > 0)
            {
                failedThreshold++;
                await App.Cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut),
                    null);
            }

            if (loginFailedLimitOptions.Enabled && loginAttempt != null)
            {
                loginAttempt.Count++;
                if (loginAttempt.Count >= loginFailedLimitOptions.MaxAttempts)
                {
                    loginAttempt.IsLocked = true;
                    loginAttempt.LockUntil = DateTime.Now.AddSeconds(loginFailedLimitOptions.Lockout);
                }


                await App.Cache.SetAsync(attempsCacheKey, loginAttempt,
                    TimeSpan.FromSeconds(loginFailedLimitOptions.Lockout), null);
            }

            return loginFailedLimitOptions.Enabled
                ? Error(App.L.R("Error.InvalidPasswordWithLockWarning"))
                : Error(App.L.R("Error.InvalidPassword"));
        }

        if (!userDto.Enabled)
        {
            if (captchaOptions.Threshold > 0)
            {
                failedThreshold++;
                await App.Cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut),
                    null);
            }

            return Error(App.L.R("Error.UserNotActivated"));
        }

        await App.Cache.RemoveAsync(authUser.CaptchaId);
        await App.Cache.RemoveAsync(thresholdCacheKey);
        await App.Cache.RemoveAsync(attempsCacheKey);
        var netUser = await _userService.QueryByIdAsync(userDto.Id);
        return await LoginResult(netUser, "login");
    }


    /// <summary>
    /// 刷新Token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("refreshToken")]
    [Description("Action.RefreshToken")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenVo))]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshToken refreshToken)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var token = refreshToken.Token;
        var tokenMd5 = token.ToMd5String16();
        var tokenBlacklist = await _tokenBlacklistService
            .TableWhere(x => x.AccessToken == tokenMd5, null, null, null, true)
            .FirstAsync();
        if (!tokenBlacklist.IsNull())
        {
            return Error(App.L.R("Error.TokenRevoked"));
        }

        var jwtSecurityToken = await _tokenService.ReadJwtToken(token);
        if (jwtSecurityToken != null)
        {
            var userId = Convert.ToInt64(jwtSecurityToken.Claims
                .FirstOrDefault(s => s.Type == AuthConstants.JwtClaimTypes.Jti)?.Value);
            var loginTime = Convert.ToInt64(jwtSecurityToken.Claims
                .FirstOrDefault(s => s.Type == AuthConstants.JwtClaimTypes.Iat)?.Value).TimeMillisecondToDateTime();
            var nowTime = DateTime.Now.ToLocalTime();
            var refreshTimeClaims = Convert.ToInt64(jwtSecurityToken.Claims
                .FirstOrDefault(s => s.Type == AuthConstants.JwtClaimTypes.RefreshTime)?.Value);
            var refreshTime = refreshTimeClaims.TimeMillisecondToDateTime();
            // 允许token刷新时间内
            if (nowTime < refreshTime)
            {
                var netUser = await _userService.QueryByIdAsync(userId);
                if (netUser.IsNotNull())
                    if (netUser.UpdateTime == null || netUser.UpdateTime < loginTime)
                        return await LoginResult(netUser, "refresh", refreshTimeClaims);
            }

            return Error(App.L.R("Error.TokenExpired"));
        }

        return Error(App.L.R("Error.TokenParseFailed"));
    }


    [HttpGet]
    [Route("info")]
    [Description("Action.PersonalInfo")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JwtUserVo))]
    public async Task<ActionResult> GetInfo()
    {
        if (App.HttpUser.IsNull() || App.HttpUser.Id == 0)
        {
            return Error(App.L.R("Error.TokenExpired"));
        }

        var netUser = await _userService.QueryByIdAsync(App.HttpUser.Id);

        var authCodeList = await _permissionService.GetAuthCodeAsync(netUser.Id);


        var jwtUserVo = await _onlineUserService.CreateJwtUserAsync(netUser, authCodeList);
        return JsonContent(jwtUserVo);
    }


    /// <summary>
    /// 获取邮箱验证码，申请变更邮箱
    /// </summary>
    /// <param name="emailCodeDto"></param>
    /// <returns></returns>
    [HttpPost]
    [Description("Action.GetEmailVerificationCode")]
    [Route("resetEmailCode")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> ResetEmailCode([FromBody] EmailCodeDto emailCodeDto)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var result = await _queuedEmailService.ResetEmailCode(emailCodeDto.Email, "EmailVerificationCode");
        return Ok(result);
    }


    /// <summary>
    /// 系统用户登出
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    [Route("logout")]
    [Description("Action.UserLogOut")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> Logout()
    {
        //清理缓存
        if (App.HttpUser.IsNotNull())
        {
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.OnlineKey +
                                        App.HttpUser.JwtToken.ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserInfoById +
                                        App.HttpUser.Id.ToString().ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserMenuById +
                                        App.HttpUser.Id.ToString().ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserAuthCodes +
                                        App.HttpUser.Id.ToString().ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserAuthUrls +
                                        App.HttpUser.Id.ToString().ToMd5String16());
            await App.Cache.RemoveAsync(GlobalConstants.CachePrefix.UserDataScopeById +
                                        App.HttpUser.Id.ToString().ToMd5String16());
            await _tokenBlacklistService.AddAsync(new Entity.Core.System.TokenBlacklist
            {
                AccessToken = App.HttpUser.JwtToken.ToMd5String16()
            });
        }

        return Ok(OperateResult.Success());
    }


    /// <summary>
    /// swagger登录
    /// </summary>
    /// <param name="swaggerLoginAuthUser"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("swagger/login")]
    [Description("Action.UserLogin")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActionResultVm))]
    public async Task<ActionResult> SwaggerLogin([FromBody] SwaggerLoginAuthUser swaggerLoginAuthUser)
    {
        if (!ModelState.IsValid)
        {
            var actionError = ModelState.GetErrors();
            return Error(actionError);
        }

        var userDto = await _userService.QueryByNameAsync(swaggerLoginAuthUser.Username);
        if (userDto == null)
        {
            return Error(App.L.R("Error.UserNotFound"));
        }

        var rsaOptions = App.GetOptions<RsaOptions>();
        var password =
            new RsaHelper(rsaOptions.PrivateKey, rsaOptions.PublicKey).Decrypt(swaggerLoginAuthUser.Password);
        if (!BCryptHelper.Verify(password, userDto.Password))
        {
            return Error(App.L.R("Error.InvalidPassword"));
        }


        if (!userDto.Enabled)
        {
            return Error(App.L.R("Error.UserNotActivated"));
        }

        App.HttpContext.Session.SetInt32("swagger-key", 1);
        return Ok(OperateResult.Success());
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 登录或刷新token相应结果
    /// </summary>
    /// <param name="userDto"></param>
    /// <param name="type">login:登录,refresh:刷新token</param>
    /// <param name="refreshTime"></param>
    /// <returns></returns>
    private async Task<ActionResult> LoginResult(UserVo userDto, string type, long refreshTime = 0)
    {
        var authCodeList = new List<string>();
        var refresh = true;
        if (type.Equals("login"))
        {
            refresh = false;
            authCodeList = await _permissionService.GetAuthCodeAsync(userDto.Id);
        }

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        //var jwtUserVo = await _onlineUserService.CreateJwtUserAsync(userDto, authCodeList);
        var loginUserInfo = await _onlineUserService.SaveLoginUserAsync(userDto, remoteIp);
        var token = await _tokenService.IssueTokenAsync(loginUserInfo, refresh, refreshTime);
        loginUserInfo.AccessToken = refresh ? token.RefreshToken : token.AccessToken;
        var onlineKey = loginUserInfo.AccessToken.ToMd5String16();
        await App.Cache.SetAsync(
            GlobalConstants.CachePrefix.OnlineKey + onlineKey,
            loginUserInfo, TimeSpan.FromHours(2), CacheExpireType.Absolute);

        switch (type)
        {
            case "login":
                var response = new LoginResultVo
                {
                    User = userDto,
                    RoleCodes = userDto.Roles.Select(x => x.AuthCode).ToList(),
                    AuthCodes = authCodeList,
                    TokenVo = token
                };
                return JsonContent(response);
            case "refresh":
                return JsonContent(token);
            default:
                return Ok(OperateResult.Error("Parameter error."));
        }
    }

    #endregion
}