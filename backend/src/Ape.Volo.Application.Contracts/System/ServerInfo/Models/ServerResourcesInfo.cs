

namespace Ape.Volo.Application.Contracts.System.ServerInfo.Models;

/// <summary>
/// 服务器资源信息
/// </summary>
public class ServerResourcesInfo
{
    /// <summary>
    /// 运行时间
    /// </summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>
    /// 系统信息
    /// </summary>
    public Sys Sys { get; set; } = default!;

    /// <summary>
    /// Cpu信息
    /// </summary>
    public Cpu Cpu { get; set; } = default!;

    /// <summary>
    /// 内存信息
    /// </summary>
    public Memory Memory { get; set; } = default!;

    /// <summary>
    /// 交换区信息
    /// </summary>
    public Swap Swap { get; set; } = default!;

    /// <summary>
    /// 磁盘信息
    /// </summary>
    public Disk Disk { get; set; } = default!;
}
