using System.IdentityModel.Tokens.Jwt;
using Ape.Volo.Common.WebApp;
using Ape.Volo.ViewModel.Jwt;

namespace Ape.Volo.Infrastructure.Authentication;

public interface ITokenService
{
    /// <summary>
    /// 颁发Token
    /// </summary>
    /// <param name="loginUserInfo"></param>
    /// <param name="refresh"></param>
    /// <param name="refreshTime"></param>
    /// <returns></returns>
    Task<TokenVo> IssueTokenAsync(LoginUserInfo loginUserInfo, bool refresh = false, long refreshTime = 0);

    /// <summary>
    /// 读取Token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<JwtSecurityToken> ReadJwtToken(string token);
}
