using System.Net;
using System.Net.Sockets;
using Ape.Volo.Application.Contracts.Abstractions;
using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;

namespace Ape.Volo.Web.Services;

/// <summary>
/// 双栈离线归属地查询。以单例持有两份全量内存缓存，查询不读取文件。
/// </summary>
public sealed class IpLocationService : IIpLocationService, IDisposable
{
    private static readonly IpLocation Empty = new();
    private static readonly IpLocation Local = new() { Country = "内网IP" };
    private readonly ISearcher? _ipv4;
    private readonly ISearcher? _ipv6;
    private readonly ILogger<IpLocationService> _logger;

    /// <summary>
    /// 加载两份 xdb；某份加载失败不影响另一协议的查询。
    /// </summary>
    /// <param name="ipv4Path">IPv4 数据库路径</param>
    /// <param name="ipv6Path">IPv6 数据库路径</param>
    /// <param name="logger">日志记录器</param>
    public IpLocationService(string ipv4Path, string ipv6Path, ILogger<IpLocationService> logger)
    {
        _logger = logger;
        _ipv4 = Load(ipv4Path);
        _ipv6 = Load(ipv6Path);
    }

    /// <inheritdoc />
    public IpLocation Search(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip) || ip != ip.Trim() || ip.Contains('[') ||
            !IPAddress.TryParse(ip, out var address)) return Empty;

        // IPAddress.TryParse 也接受整数、十六进制和省略段形式，这里只接受标准点分 IPv4。
        if (address.AddressFamily == AddressFamily.InterNetwork &&
            (ip.Split('.') is not { Length: 4 } parts ||
             parts.Any(part => part.Length == 0 || part.Any(c => c < '0' || c > '9') ||
                               (part.Length > 1 && part[0] == '0')))) return Empty;

        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        if (IsLocal(address)) return Local;

        var searcher = address.AddressFamily == AddressFamily.InterNetwork ? _ipv4 : _ipv6;
        if (searcher == null) return Empty;
        try
        {
            var region = searcher.Search(address);
            if (string.IsNullOrEmpty(region)) return Empty;
            var fields = region.Split('|');
            if (fields.Length != 5) return Empty;
            static string Value(string field) => field == "0" ? string.Empty : field.Trim();
            return new IpLocation
            {
                Country = Value(fields[0]), Province = Value(fields[1]), City = Value(fields[2]),
                Isp = Value(fields[3]), CountryCode = Value(fields[4])
            };
        }
        catch (Exception exception)
        {
            // 归属地是辅助信息，数据库损坏也不能中断登录或日志处理。
            _logger.LogWarning(exception, "IP 归属地查询失败：{Ip}", ip);
            return Empty;
        }
    }

    private ISearcher? Load(string path)
    {
        try
        {
            return new Searcher(CachePolicy.Content, path);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "IP 归属地数据库加载失败：{Path}", path);
            return null;
        }
    }

    private static bool IsLocal(IPAddress address)
    {
        if (IPAddress.IsLoopback(address)) return true;
        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
            return bytes[0] is 0 or 10 or 127 ||
                   (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168) ||
                   (bytes[0] == 169 && bytes[1] == 254) ||
                   (bytes[0] == 100 && bytes[1] is >= 64 and <= 127) || bytes[0] >= 224;

        return address.Equals(IPAddress.IPv6Any) || address.IsIPv6LinkLocal ||
               address.IsIPv6SiteLocal || address.IsIPv6Multicast || (bytes[0] & 0xfe) == 0xfc;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _ipv4?.Dispose();
        _ipv6?.Dispose();
    }
}
