using IP2Region.Net.Abstractions;
using Shyjus.BrowserDetection;

namespace Ape.Volo.Web.Authentication;

/// <summary>
/// 基于当前 HTTP 请求解析客户端环境：IP 取自连接地址，归属地查 ip2region，其余取自 UA。
/// </summary>
public sealed class ClientEnvironment : IClientEnvironment
{
    private readonly ISearcher _ipSearcher;
    private readonly IBrowserDetector _browserDetector;

    /// <summary>
    /// 初始化 <see cref="ClientEnvironment"/> 的新实例。
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器</param>
    /// <param name="ipSearcher">IP 归属地查询</param>
    /// <param name="browserDetector">UA 解析</param>
    public ClientEnvironment(IHttpContextAccessor httpContextAccessor, ISearcher ipSearcher,
        IBrowserDetector browserDetector)
    {
        _ipSearcher = ipSearcher;
        _browserDetector = browserDetector;
        IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
    }

    /// <inheritdoc />
    public string IpAddress { get; }

    /// <inheritdoc />
    public string Location => _ipSearcher.Search(IpAddress) ?? string.Empty;

    /// <inheritdoc />
    public string OperatingSystem => _browserDetector.Browser?.OS ?? string.Empty;

    /// <inheritdoc />
    public string DeviceType => _browserDetector.Browser?.DeviceType ?? string.Empty;

    /// <inheritdoc />
    public string BrowserName => _browserDetector.Browser?.Name ?? string.Empty;

    /// <inheritdoc />
    public string BrowserVersion => _browserDetector.Browser?.Version ?? string.Empty;
}
