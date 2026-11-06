namespace Ape.Volo.Application.Contracts.Abstractions;

/// <summary>
/// 离线 IP 归属地查询，不依赖具体数据库实现。
/// </summary>
public interface IIpLocationService
{
    /// <summary>
    /// 查询 IPv4 或 IPv6；无效地址、无记录或数据库不可用时返回空结果。
    /// 内网地址返回国家为“内网IP”的结果。
    /// </summary>
    /// <param name="ip">IP 地址，不包含端口</param>
    /// <returns>统一的归属地信息</returns>
    IpLocation Search(string? ip);
}
