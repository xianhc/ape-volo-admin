using Ape.Volo.Application.Contracts.Authorization;
using Ape.Volo.Application.Contracts.System.ServerInfo;
using Ape.Volo.Application.Contracts.System.ServerInfo.Models;
using Ape.Volo.Common.Extensions;

namespace Ape.Volo.Core.System.ServerInfo;

/// <summary>
/// 服务器资源服务
/// </summary>
public class ServerResourcesService : IServerResourcesService
{
    private const int B = 1;
    private const int Kb = 1024 * B;
    private const int Mb = 1024 * Kb;
    private const int Gb = 1024 * Mb;

    /// <summary>
    /// 按总量与可用量计算使用率
    /// </summary>
    /// <remarks>
    /// 探测失败时总量可能为 0（容器裁剪 /proc、WMI 被禁用等），
    /// 直接相除会抛 DivideByZeroException 把整个接口打成 500，这里统一兜底为 0。
    /// </remarks>
    /// <param name="total">总量（字节）</param>
    /// <param name="free">可用量（字节）</param>
    /// <returns>保留两位小数的百分比</returns>
    private static string UsageRate(long total, long free)
    {
        if (total <= 0)
        {
            return "0.00";
        }

        var used = Math.Clamp(total - free, 0, total);
        return (used * 100.0 / total).ToString("0.00");
    }

    /// <inheritdoc />
    [HasPerm(SystemPermissions.Server.List)]
    public async Task<ServerResourcesInfo> GetList(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var os = new OsInfo();

        ServerResourcesInfo osInfo = new ServerResourcesInfo
        {
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Sys =
                new Sys
                {
                    Os = os.OsDescription, Day = os.TickCount.FormatLongToTime(), Ip = os.LocalIp,
                },
            Cpu = new Cpu
            {
                Name = os.ProcessorName,
                Package = os.PhysicalProcessorCount + " physical CPUs",
                Core = os.NumberOfCores + " physical cores",
                CoreNumber = os.NumberOfCores,
                Logic = os.LogicProcessorCount + " logical CPUs",
                Used = os.ProcessorUtilizationRate.ToString(),
                Idle = (100 - os.ProcessorUtilizationRate).ToString()
            },
            Memory = new Memory
            {
                Total = Math.Round(Convert.ToDecimal(os.TotalPhysicalMemory) / Gb, 2)
                    .ToString("0.00") + " GiB",
                Used = Math.Round(Convert.ToDecimal(os.TotalPhysicalMemory - os.FreePhysicalMemory) / 1073741824, 2)
                    .ToString("0.00") + " GiB",
                Available = Math.Round(Convert.ToDecimal(os.FreePhysicalMemory) / Gb, 1)
                    .ToString("0.00") + " GiB",
                UsageRate = UsageRate(os.TotalPhysicalMemory, os.FreePhysicalMemory)
            },
            Swap = new Swap(),
            Disk = new Disk()
        };
        if (os.LogicalDisk is { Count: > 0 })
        {
            long size = 0;
            long freeSpace = 0;
            foreach (var item in os.LogicalDisk)
            {
                size += item.Size;
                freeSpace += item.FreeSpace;
            }

            osInfo.Disk.Total = Math.Round(Convert.ToDecimal(size) / Gb, 2).ToString("0.00") +
                                " GiB";
            osInfo.Disk.Used =
                Math.Round(Convert.ToDecimal(size - freeSpace) / Gb, 2).ToString("0.00") + " GiB";
            osInfo.Disk.Available =
                Math.Round(Convert.ToDecimal(freeSpace) / Gb, 2).ToString("0.00") + " GiB";
            osInfo.Disk.UsageRate = UsageRate(size, freeSpace);
        }

        if (os.SwapTotal > 0)
        {
            osInfo.Swap.Total =
                Math.Round(Convert.ToDecimal((int)(os.SwapTotal / Gb)), 2).ToString("0.00") + " GiB";
            osInfo.Swap.Used = Math.Round(Convert.ToDecimal((int)((os.SwapTotal - os.SwapFree) / Gb)), 2)
                .ToString("0.00") + " GiB";
            osInfo.Swap.Available =
                Math.Round(Convert.ToDecimal((int)(os.SwapFree / Gb)), 2).ToString("0.00") + " GiB";
            osInfo.Swap.UsageRate = UsageRate(os.SwapTotal, os.SwapFree);
        }

        await Task.CompletedTask;
        return osInfo;
    }
}
