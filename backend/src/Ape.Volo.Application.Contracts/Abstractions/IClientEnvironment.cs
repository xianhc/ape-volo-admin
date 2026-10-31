namespace Ape.Volo.Application.Contracts.Abstractions;

/// <summary>
/// 当前请求的客户端环境：IP、归属地、操作系统、设备与浏览器信息。
/// 应用层只依赖该抽象，具体解析（IP 库、UA 解析）由宿主层实现。
/// </summary>
public interface IClientEnvironment
{
    /// <summary>
    /// 客户端 IP
    /// </summary>
    string IpAddress { get; }

    /// <summary>
    /// IP 归属地
    /// </summary>
    string Location { get; }

    /// <summary>
    /// 操作系统
    /// </summary>
    string OperatingSystem { get; }

    /// <summary>
    /// 设备类型
    /// </summary>
    string DeviceType { get; }

    /// <summary>
    /// 浏览器名称
    /// </summary>
    string BrowserName { get; }

    /// <summary>
    /// 浏览器版本
    /// </summary>
    string BrowserVersion { get; }
}
