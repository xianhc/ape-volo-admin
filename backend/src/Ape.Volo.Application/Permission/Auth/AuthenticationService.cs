using Ape.Volo.Application.Contracts.Permission.Users.Models;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Helper;
using Ape.Volo.Common.IdGenerator;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Utils;
using Ape.Volo.Shared.Exceptions;

namespace Ape.Volo.Application.Permission.Auth;

/// <summary>
/// 登录用例实现：验证码阈值、失败计数、账号锁定与凭据校验。
/// </summary>
/// <remarks>
/// 失败一律通过 <see cref="LoginResult"/> 返回，不抛 <see cref="BadRequestException"/>——
/// 抛异常会被宿主的异常过滤器写进异常日志表，而密码输错属于正常业务事件。
/// </remarks>
public class AuthenticationService : IAuthenticationService
{
    #region 字段

    private readonly IUserLookupService _users;
    private readonly IPermissionService _permissions;
    private readonly IOnlineUserService _onlineUsers;
    private readonly ITokenIssuer _tokens;
    private readonly ICache _cache;
    private readonly IClientEnvironment _client;
    private readonly ICaptchaImageGenerator _captchaImages;
    private readonly IOptions<CaptchaOptions> _captchaOptions;
    private readonly IOptions<LoginFailedLimitOptions> _limitOptions;
    private readonly IOptions<SystemOptions> _systemOptions;
    private readonly IPasswordDecryptor _passwordDecryptor;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化 <see cref="AuthenticationService"/> 的新实例。
    /// </summary>
    /// <param name="users">用户查询</param>
    /// <param name="permissions">权限码查询</param>
    /// <param name="onlineUsers">在线用户</param>
    /// <param name="tokens">令牌签发</param>
    /// <param name="cache">缓存</param>
    /// <param name="client">客户端环境</param>
    /// <param name="captchaImages">验证码图片生成</param>
    /// <param name="captchaOptions">验证码配置</param>
    /// <param name="limitOptions">登录失败限制配置</param>
    /// <param name="systemOptions">系统配置</param>
    /// <param name="passwordDecryptor">密码传输解密器</param>
    public AuthenticationService(IUserLookupService users, IPermissionService permissions, IOnlineUserService onlineUsers,
        ITokenIssuer tokens, ICache cache, IClientEnvironment client, ICaptchaImageGenerator captchaImages,
        IOptions<CaptchaOptions> captchaOptions, IOptions<LoginFailedLimitOptions> limitOptions,
        IOptions<SystemOptions> systemOptions, IPasswordDecryptor passwordDecryptor)
    {
        _users = users;
        _permissions = permissions;
        _onlineUsers = onlineUsers;
        _tokens = tokens;
        _cache = cache;
        _client = client;
        _captchaImages = captchaImages;
        _captchaOptions = captchaOptions;
        _limitOptions = limitOptions;
        _systemOptions = systemOptions;
        _passwordDecryptor = passwordDecryptor;
    }

    #endregion

    #region 验证码

    /// <inheritdoc />
    public async Task<CaptchaVo> CreateCaptchaAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var captchaOptions = _captchaOptions.Value;

        var showCaptcha = true;
        if (captchaOptions.Threshold > 0)
        {
            var thresholdCacheKey = ThresholdCacheKey;
            var failedThreshold = await _cache.GetAsync<int>(thresholdCacheKey);
            if (failedThreshold <= 0)
            {
                failedThreshold = 1;
                await _cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut), null);
            }

            showCaptcha = failedThreshold > captchaOptions.Threshold;
        }

        var image = _captchaImages.Create(captchaOptions.ImgWidth, captchaOptions.ImgHeight,
            captchaOptions.FontSize, captchaOptions.KeyLength);
        var captchaId = GlobalConstants.CachePrefix.CaptchaId +
                        IdHelper.NextId().ToString().Base64Encode();
        await _cache.SetAsync(captchaId, image.Code, TimeSpan.FromMinutes(2), null);

        return new CaptchaVo { Img = image.ImageDataUrl, CaptchaId = captchaId, ShowCaptcha = showCaptcha };
    }

    #endregion

    #region 登录

    /// <inheritdoc />
    public async Task<LoginResult> LoginAsync(LoginAuthUser authUser, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var captchaOptions = _captchaOptions.Value;
        var limitOptions = _limitOptions.Value;

        var attemptsCacheKey = GlobalConstants.CachePrefix.Attempts + _client.IpAddress + authUser.UserName;
        LoginAttempt? loginAttempt = null;
        if (limitOptions.Enabled)
        {
            loginAttempt = await _cache.GetAsync<LoginAttempt>(attemptsCacheKey);
            if (loginAttempt.IsNull())
            {
                loginAttempt = new LoginAttempt { Count = 0, IsLocked = false, LockUntil = DateTime.MinValue };
                await _cache.SetAsync(attemptsCacheKey, loginAttempt,
                    TimeSpan.FromSeconds(limitOptions.Lockout), null);
            }

            if (loginAttempt.IsLocked && DateTime.Now < loginAttempt.LockUntil)
            {
                return LoginResult.Failure(
                    $"账户已锁定，请稍后重试。解锁时间：{loginAttempt.LockUntil:yyyy-MM-dd HH:mm:ss}");
            }
        }

        var showCaptcha = true;
        var thresholdCacheKey = ThresholdCacheKey;
        var failedThreshold = 0;

        // 登录失败统一处理：图形验证码阈值与账号锁定计数必须走同一套逻辑，
        // 否则「用户不存在」与「密码错误」的响应内容/副作用不一致，会被用来枚举账号
        async Task RecordLoginFailureAsync()
        {
            if (captchaOptions.Threshold > 0)
            {
                failedThreshold++;
                await _cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut), null);
            }

            if (!limitOptions.Enabled || loginAttempt == null)
            {
                return;
            }

            loginAttempt.Count++;
            if (loginAttempt.Count >= limitOptions.MaxAttempts)
            {
                loginAttempt.IsLocked = true;
                loginAttempt.LockUntil = DateTime.Now.AddSeconds(limitOptions.Lockout);
            }

            await _cache.SetAsync(attemptsCacheKey, loginAttempt,
                TimeSpan.FromSeconds(limitOptions.Lockout), null);
        }

        string LoginFailureMessage() => limitOptions.Enabled
            ? "用户名或密码错误，连续输入错误将锁定账号！"
            : "用户名或密码错误";

        if (captchaOptions.Threshold > 0)
        {
            failedThreshold = await _cache.GetAsync<int>(thresholdCacheKey);
            if (failedThreshold <= 0)
            {
                failedThreshold = 1;
                await _cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut), null);
            }

            showCaptcha = failedThreshold > captchaOptions.Threshold;
        }

        if (_systemOptions.Value.RunMode != RunMode.Dev && showCaptcha)
        {
            var captchaFailure = await ValidateCaptchaAsync(authUser, captchaOptions, thresholdCacheKey,
                failedThreshold, ct);
            if (captchaFailure != null)
            {
                return captchaFailure;
            }
        }

        var userDto = await _users.FindByNameAsync(authUser.UserName, ct);
        if (userDto == null)
        {
            await RecordLoginFailureAsync();
            return LoginResult.Failure(LoginFailureMessage());
        }

        // 密文由客户端提供；格式非法时与密码错误同等处理，不区分出「解密失败」这一分支
        if (!_passwordDecryptor.TryDecrypt(authUser.Password, out var password))
        {
            await RecordLoginFailureAsync();
            return LoginResult.Failure(LoginFailureMessage());
        }

        if (!BCryptHelper.Verify(password, userDto.Password))
        {
            await RecordLoginFailureAsync();
            return LoginResult.Failure(LoginFailureMessage());
        }

        if (!userDto.Enabled)
        {
            if (captchaOptions.Threshold > 0)
            {
                failedThreshold++;
                await _cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut), null);
            }

            return LoginResult.Failure("用户未激活");
        }

        await _cache.RemoveAsync(thresholdCacheKey);
        await _cache.RemoveAsync(attemptsCacheKey);

        return await BuildLoginResultAsync(userDto, ct);
    }

    /// <summary>
    /// 校验图形验证码；通过时消费掉验证码并返回 null。
    /// </summary>
    /// <param name="authUser">登录请求</param>
    /// <param name="captchaOptions">验证码配置</param>
    /// <param name="thresholdCacheKey">阈值缓存键</param>
    /// <param name="failedThreshold">当前失败阈值计数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>校验失败的结果；通过时为 null</returns>
    private async Task<LoginResult?> ValidateCaptchaAsync(LoginAuthUser authUser, CaptchaOptions captchaOptions,
        string thresholdCacheKey, int failedThreshold, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (authUser.Captcha.IsNullOrEmpty())
        {
            return LoginResult.Failure(ValidationError.Required(authUser, nameof(authUser.Captcha)));
        }

        if (authUser.CaptchaId.IsNullOrEmpty())
        {
            return LoginResult.Failure(ValidationError.Required(authUser, nameof(authUser.CaptchaId)));
        }

        var code = await _cache.GetAsync<string>(authUser.CaptchaId);
        if (code.IsNullOrEmpty())
        {
            return LoginResult.Failure("验证码已过期");
        }

        if (!code.Equals(authUser.Captcha))
        {
            if (captchaOptions.Threshold > 0)
            {
                failedThreshold++;
                await _cache.SetAsync(thresholdCacheKey, failedThreshold,
                    TimeSpan.FromSeconds(captchaOptions.TimeOut), null);
            }

            return LoginResult.Failure("验证码已过期");
        }

        // 验证码一次性消费：校验通过即失效，避免同一验证码被复用做撞库
        await _cache.RemoveAsync(authUser.CaptchaId);
        return null;
    }

    /// <summary>
    /// 凭据校验通过后组装登录结果并签发令牌。
    /// </summary>
    /// <param name="userDto">已通过校验的用户</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>登录结果</returns>
    private async Task<LoginResult> BuildLoginResultAsync(UserVo userDto, CancellationToken ct)
    {
        var netUser = await _users.FindByIdAsync(userDto.Id, ct);
        if (!netUser.Enabled)
        {
            return LoginResult.Unauthorized();
        }

        var authCodes = await _permissions.GetAuthCodeAsync(netUser.Id, ct);
        var loginInfo = await _onlineUsers.SaveLoginUserAsync(netUser, ct);
        var token = await _tokens.IssueAsync(loginInfo, ct);
        if (token == null)
        {
            return LoginResult.Unauthorized("用户凭据或状态已变化，请重新登录。");
        }

        return LoginResult.Success(new LoginResultVo
        {
            User = netUser,
            RoleCodes = netUser.Roles.Select(x => x.AuthCode ?? string.Empty).ToList(),
            AuthCodes = authCodes,
            TokenVo = token
        });
    }

    #endregion

    #region 当前用户信息

    /// <inheritdoc />
    public async Task<JwtUserVo> GetCurrentUserInfoAsync(long userId, CancellationToken ct = default)
    {
        var netUser = await _users.FindByIdAsync(userId, ct);
        var authCodeList = await _permissions.GetAuthCodeAsync(netUser.Id, ct);
        return await _onlineUsers.CreateJwtUserAsync(netUser, authCodeList, ct);
    }

    #endregion

    /// <summary>
    /// 图形验证码阈值缓存键：按客户端 IP 计数。
    /// </summary>
    private string ThresholdCacheKey => GlobalConstants.CachePrefix.Threshold + _client.IpAddress;
}
