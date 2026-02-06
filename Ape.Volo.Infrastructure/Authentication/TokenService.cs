using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Common.WebApp;
using Ape.Volo.Core;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.ViewModel.Jwt;
using Dm.util;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Ape.Volo.Infrastructure.Authentication;

/// <summary>
/// 
/// </summary>
public class TokenService : ITokenService
{
    private readonly ILogger<TokenService> _logger;


    public TokenService(ILogger<TokenService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 颁发Token
    /// </summary>
    /// <param name="loginUserInfo"></param>
    /// <param name="refresh"></param>
    /// <param name="refreshTime"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<TokenVo> IssueTokenAsync(LoginUserInfo loginUserInfo, bool refresh = false, long refreshTime = 0)
    {
        if (loginUserInfo == null)
            throw new ArgumentNullException(nameof(loginUserInfo));

        var jwtAuthOptions = App.GetOptions<JwtAuthOptions>();
        var signinCredentials =
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.SecurityKey)),
                SecurityAlgorithms.HmacSha256);
        var nowTime = DateTime.Now;
        if (refreshTime == 0)
        {
            refreshTime = nowTime.AddMinutes(jwtAuthOptions.RefreshTokenExpires).ToUnixTimeStampMillisecond();
        }
        var cls = new List<Claim>
        {
            new(AuthConstants.JwtClaimTypes.Jti, loginUserInfo.UserId.ToString()),
            new(AuthConstants.JwtClaimTypes.Name, loginUserInfo.Account),
            new(AuthConstants.JwtClaimTypes.TenantId, loginUserInfo.TenantId.ToString()),
            new(AuthConstants.JwtClaimTypes.DeptId, loginUserInfo.DeptId.ToString()),
            new(AuthConstants.JwtClaimTypes.Iat, nowTime.ToUnixTimeStampMillisecond().ToString()),
            new(AuthConstants.JwtClaimTypes.Ip, loginUserInfo.Ip),
            new(AuthConstants.JwtClaimTypes.RefreshTime,refreshTime.toString())
        };
        var identity = new ClaimsIdentity(AuthConstants.JwtTokenType);
        identity.AddClaims(cls);


        var tokeOptions = new JwtSecurityToken(
            issuer: jwtAuthOptions.Issuer,
            audience: jwtAuthOptions.Audience,
            claims: cls,
            notBefore: nowTime,
            expires: nowTime.AddMinutes(jwtAuthOptions.Expires),
            signingCredentials: signinCredentials
        );

        var expires = nowTime.AddMinutes(jwtAuthOptions.Expires).ToUnixTimeStampMillisecond();
        var token = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        if (refresh)
        {
            return await Task.FromResult(new TokenVo
            {
                Expires = expires,
                TokenType = AuthConstants.JwtTokenType,
                RefreshToken = token,
                RefreshTokenExpires = refreshTime
            });
        }

        return await Task.FromResult(new TokenVo
        {
            AccessToken = token,
            Expires = expires,
            TokenType = AuthConstants.JwtTokenType,
            RefreshToken = "",
            RefreshTokenExpires = refreshTime
        });
    }


    /// <summary>
    /// 读取Token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<JwtSecurityToken> ReadJwtToken(string token)
    {
        try
        {
            token = token.Replace(AuthConstants.JwtTokenType, "").Trim();
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            if (jwtSecurityTokenHandler.CanReadToken(token))
            {
                var jwtAuthOptions = App.GetOptions<JwtAuthOptions>();
                var signinCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.SecurityKey)),
                        SecurityAlgorithms.HmacSha256);
                JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(token);
                var rawSignature = JwtTokenUtilities.CreateEncodedSignature(
                    jwtSecurityToken.RawHeader + "." + jwtSecurityToken.RawPayload,
                    signinCredentials);
                if (jwtSecurityToken.RawSignature == rawSignature)
                {
                    return await Task.FromResult(jwtSecurityToken);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error reading JWT token: {Message}", e.Message);
        }

        return null;
    }
}
