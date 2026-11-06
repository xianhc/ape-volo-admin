namespace Ape.Volo.Application.Contracts.Abstractions;

/// <summary>
/// IP 归属地信息；未知字段为空字符串。
/// </summary>
public sealed record IpLocation
{
    /// <summary>
    /// 国家
    /// </summary>
    public string Country { get; init; } = string.Empty;

    /// <summary>
    /// 省份
    /// </summary>
    public string Province { get; init; } = string.Empty;

    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// 网络服务提供商
    /// </summary>
    public string Isp { get; init; } = string.Empty;

    /// <summary>
    /// ISO 3166-1 alpha-2 国家代码
    /// </summary>
    public string CountryCode { get; init; } = string.Empty;

    /// <summary>
    /// 以竖线分隔五个字段，供现有登录环境及日志使用。
    /// </summary>
    public override string ToString() => Country.Length + Province.Length + City.Length + Isp.Length + CountryCode.Length == 0
        ? string.Empty
        : string.Join("|", Country, Province, City, Isp, CountryCode);
}
