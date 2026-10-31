using System.Globalization;

namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 将当前 HTTP 请求的已认证声明适配为业务身份；由宿主按请求作用域注册。
/// </summary>
/// <param name="accessor">HTTP 上下文访问器</param>
public sealed class HttpUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private readonly HttpContext? _context = accessor.HttpContext;

    /// <inheritdoc />
    public bool IsAuthenticated => _context?.User.Identity?.IsAuthenticated == true;
    /// <inheritdoc />
    public long Id => ReadId(AuthConstants.JwtClaimTypes.Sub);
    /// <inheritdoc />
    public string Account => IsAuthenticated ? _context?.User.FindFirst(AuthConstants.JwtClaimTypes.Name)?.Value ?? string.Empty : string.Empty;
    /// <inheritdoc />
    public long DeptId => ReadId(AuthConstants.JwtClaimTypes.DeptId);

    private long ReadId(string claim) => IsAuthenticated && long.TryParse(_context?.User.FindFirst(claim)?.Value,
        NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : 0;
}
