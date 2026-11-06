using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Ape.Volo.Core.System.ServerInfo;

/// <summary>
/// 系统信息
/// </summary>
public class OsInfo
{
    /// <summary>
    /// 确定当前操作系统是否为64位操作系统
    /// </summary>
    public bool Is64BitOperatingSystem { get; set; } = Environment.Is64BitOperatingSystem;

    /// <summary>
    /// 获取此本地计算机的NetBIOS名称
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;

    /// <summary>
    /// 获取当前平台标识符和版本号
    /// </summary>
    public OperatingSystem OsVersion { get; set; } = Environment.OSVersion;

    /// <summary>
    /// 获取当前计算机上的物理处理器核心数量
    /// </summary>
    public int NumberOfCores { get; set; }

    /// <summary>
    /// 获取当前计算机上的物理处理器数量
    /// </summary>
    public int PhysicalProcessorCount { get; set; }

    /// <summary>
    /// 获取当前计算机上的逻辑处理器数量
    /// </summary>
    public int LogicProcessorCount { get; set; }

    /// <summary>
    /// 处理器利用率
    /// </summary>
    public int ProcessorUtilizationRate { get; set; }

    /// <summary>
    /// 处理器名称
    /// </summary>
    public string ProcessorName { get; set; } = string.Empty;

    /// <summary>
    /// 获取系统目录的标准路径
    /// </summary>
    public string SystemDirectory { get; set; } = Environment.SystemDirectory;

    /// <summary>
    /// 获取操作系统的内存页面中的字节数
    /// </summary>
    public int SystemPageSize { get; set; } = Environment.SystemPageSize;

    /// <summary>
    /// 获取自系统启动以来经过的毫秒数
    /// </summary>
    public long TickCount { get; set; }

    /// <summary>
    /// 获取与当前用户关联的网络域名
    /// </summary>
    public string UserDomainName { get; set; } = Environment.UserDomainName;

    /// <summary>
    /// 获取当前登录到操作系统的用户的用户名
    /// </summary>
    public string UserName { get; set; } = Environment.UserName;

    /// <summary>
    /// 获取公共语言运行时的主要，次要，内部和修订版本号
    /// </summary>
    public Version Version { get; set; } = Environment.Version;

    /// <summary>
    /// 获取运行应用程序的.NET安装的名称
    /// </summary>
    public string FrameworkDescription { get; set; } = RuntimeInformation.FrameworkDescription;

    /// <summary>
    /// 获取描述应用程序正在运行的操作系统的字符串
    /// </summary>
    public string OsDescription { get; set; } = RuntimeInformation.OSDescription;

    /// <summary>
    /// 代表操作系统平台
    /// </summary>
    public string Os { get; set; } = string.Empty;

    /// <summary>
    /// 总物理内存 B
    /// </summary>
    public long TotalPhysicalMemory { get; set; }

    /// <summary>
    /// 可用物理内存 B
    /// </summary>
    public long FreePhysicalMemory { get; set; }

    /// <summary>
    /// 总交换空间（Linux）B
    /// </summary>
    public long SwapTotal { get; set; }

    /// <summary>
    /// 可用交换空间（Linux）B
    /// </summary>
    public long SwapFree { get; set; }

    /// <summary>
    /// 逻辑磁盘
    /// </summary>
    public List<DiskInfo> LogicalDisk { get; set; } = new();

    /// <summary>
    /// 本机IP
    /// </summary>
    public string LocalIp { get; set; } = string.Empty;

    /// <summary>
    /// 构造
    /// </summary>
    /// <remarks>
    /// 跨平台采集：Linux 读 /proc，Windows 走 WMI，macOS 走 sysctl；
    /// 任何一项读不到都降级为 0 或空串，绝不向外抛异常，避免服务监控页整页报错。
    /// </remarks>
    public OsInfo()
    {
        Os = ResolvePlatformName();

        // 运行时长、本机 IP、磁盘都可以用托管 API 统一拿到，不再依赖 wmic / net statistics / df。
        TickCount = Environment.TickCount64;
        LocalIp = ProbeEnvironment.LocalIpv4();
        LogicalDisk = ProbeEnvironment.LogicalDisks();

        if (OperatingSystem.IsWindows())
        {
            PlatformForWindows.Fill(this);
        }
        else if (OperatingSystem.IsLinux())
        {
            PlatformForLinux.Fill(this);
        }
        else
        {
            PlatformForUnix.Fill(this);
        }
    }

    /// <summary>
    /// 取得当前平台的名称
    /// </summary>
    /// <returns></returns>
    private static string ResolvePlatformName()
    {
        if (OperatingSystem.IsWindows())
        {
            return OSPlatform.Windows.ToString();
        }

        if (OperatingSystem.IsLinux())
        {
            return OSPlatform.Linux.ToString();
        }

        if (OperatingSystem.IsMacOS())
        {
            return OSPlatform.OSX.ToString();
        }

        return RuntimeInformation.OSDescription;
    }

    /// <summary>
    /// 与平台无关的采集项
    /// </summary>
    internal static class ProbeEnvironment
    {
        /// <summary>
        /// 容器与内存文件系统，统计物理磁盘容量时应排除
        /// </summary>
        private static readonly HashSet<string> VirtualFileSystems = new(StringComparer.OrdinalIgnoreCase)
        {
            "overlay", "tmpfs", "devtmpfs", "squashfs", "ramfs", "aufs", "proc", "sysfs", "cgroup", "cgroup2", "nsfs"
        };

        /// <summary>
        /// 获取本机 IPv4 地址
        /// </summary>
        /// <returns></returns>
        public static string LocalIpv4()
        {
            try
            {
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    foreach (var address in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }

                        if (IPAddress.IsLoopback(address.Address))
                        {
                            continue;
                        }

                        return address.Address.ToString();
                    }
                }
            }
            catch
            {
                // 读不到网卡信息时保持空串，不影响其它指标
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取固定磁盘信息
        /// </summary>
        /// <returns></returns>
        public static List<DiskInfo> LogicalDisks()
        {
            var disks = new List<DiskInfo>();

            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (!drive.IsReady || drive.DriveType != DriveType.Fixed)
                        {
                            continue;
                        }

                        // Linux 上容器镜像层、内存盘也会被枚举成 Fixed，按文件系统过滤掉
                        if (VirtualFileSystems.Contains(drive.DriveFormat ?? string.Empty))
                        {
                            continue;
                        }

                        disks.Add(new DiskInfo
                        {
                            Name = drive.Name,
                            Size = drive.TotalSize,
                            FreeSpace = drive.AvailableFreeSpace
                        });
                    }
                    catch
                    {
                        // 单个挂载点不可读（权限不足或正在卸载）时跳过
                    }
                }
            }
            catch
            {
                // 枚举驱动器失败时返回空集合
            }

            return disks;
        }
    }

    /// <summary>
    /// Windows 系统
    /// </summary>
    /// <remarks>
    /// 不走 WMI：WMI 首次查询要 2 秒以上（服务监控每 3 秒轮询一次，会直接拖垮接口），
    /// 且 wmic / net statistics 在 Windows 11 已弃用、输出解析也不稳定。
    /// 这里用 kernel32 的 P/Invoke + 注册表，单项都是微秒级。
    /// </remarks>
    [SupportedOSPlatform("windows")]
    internal static class PlatformForWindows
    {
        /// <summary>
        /// 与 PROCESSOR_RELATIONSHIP 的 RelationProcessorCore 对应
        /// </summary>
        private const int RelationProcessorCore = 0;

        /// <summary>
        /// 与 PROCESSOR_RELATIONSHIP 的 RelationProcessorPackage 对应（物理 CPU 封装）
        /// </summary>
        private const int RelationProcessorPackage = 3;

        /// <summary>
        /// 两次采样系统时间的间隔，用于计算瞬时 CPU 利用率
        /// </summary>
        private const int CpuSampleIntervalMs = 100;

        /// <summary>
        /// 采集 Windows 指标
        /// </summary>
        /// <param name="os"></param>
        public static void Fill(OsInfo os)
        {
            var memory = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
            if (GlobalMemoryStatusEx(ref memory))
            {
                os.TotalPhysicalMemory = (long)memory.TotalPhys;
                os.FreePhysicalMemory = (long)memory.AvailPhys;
            }

            os.ProcessorName = ReadRegistryString(
                @"HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString");
            os.PhysicalProcessorCount = CountProcessorPackages();
            os.LogicProcessorCount = Environment.ProcessorCount;
            os.NumberOfCores = CountPhysicalCores();
            os.ProcessorUtilizationRate = ReadCpuUsage();
        }

        /// <summary>
        /// 通过 GetSystemTimes 两次采样计算 CPU 利用率
        /// </summary>
        /// <returns></returns>
        private static int ReadCpuUsage()
        {
            try
            {
                if (!GetSystemTimes(out var idle1, out var kernel1, out var user1))
                {
                    return 0;
                }

                Thread.Sleep(CpuSampleIntervalMs);

                if (!GetSystemTimes(out var idle2, out var kernel2, out var user2))
                {
                    return 0;
                }

                var idle = idle2 - idle1;
                // kernel 时间已包含 idle，所以总时间就是 kernel + user
                var total = kernel2 - kernel1 + (user2 - user1);

                if (total == 0 || idle > total)
                {
                    return 0;
                }

                var usage = (int)Math.Round((total - idle) * 100.0 / total);
                return Math.Clamp(usage, 0, 100);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 通过 GetLogicalProcessorInformation 统计物理核心数
        /// </summary>
        /// <returns></returns>
        private static int CountPhysicalCores()
        {
            try
            {
                uint length = 0;
                GetLogicalProcessorInformation(IntPtr.Zero, ref length);
                if (length == 0)
                {
                    return 0;
                }

                var buffer = Marshal.AllocHGlobal((int)length);
                try
                {
                    if (!GetLogicalProcessorInformation(buffer, ref length))
                    {
                        return 0;
                    }

                    var size = Marshal.SizeOf<SystemLogicalProcessorInformation>();
                    if (size <= 0)
                    {
                        return 0;
                    }

                    var count = (int)(length / size);
                    var cores = 0;

                    for (var i = 0; i < count; i++)
                    {
                        var info = Marshal.PtrToStructure<SystemLogicalProcessorInformation>(
                            IntPtr.Add(buffer, i * size));
                        if (info.Relationship == RelationProcessorCore)
                        {
                            cores++;
                        }
                    }

                    return cores;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 通过 GetLogicalProcessorInformationEx 统计物理 CPU 封装数量
        /// </summary>
        /// <remarks>
        /// 不能用注册表 HARDWARE\DESCRIPTION\System\CentralProcessor 的子键数：
        /// 那是按逻辑处理器枚举的（20 线程就有 20 个子键）。
        /// </remarks>
        /// <returns></returns>
        private static int CountProcessorPackages()
        {
            try
            {
                uint length = 0;
                GetLogicalProcessorInformationEx(RelationProcessorPackage, IntPtr.Zero, ref length);
                if (length == 0)
                {
                    return 0;
                }

                var buffer = Marshal.AllocHGlobal((int)length);
                try
                {
                    if (!GetLogicalProcessorInformationEx(RelationProcessorPackage, buffer, ref length))
                    {
                        return 0;
                    }

                    var offset = 0;
                    var packages = 0;

                    // 每个条目前 8 字节固定是 Relationship(4) + Size(4)，按 Size 步进即可
                    while (offset + 8 <= length)
                    {
                        var relationship = Marshal.ReadInt32(buffer, offset);
                        var size = Marshal.ReadInt32(buffer, offset + 4);

                        if (size <= 0)
                        {
                            break;
                        }

                        if (relationship == RelationProcessorPackage)
                        {
                            packages++;
                        }

                        offset += size;
                    }

                    return packages;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 读取注册表字符串值
        /// </summary>
        /// <param name="subKey"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string ReadRegistryString(string subKey, string name)
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(subKey);
                return key?.GetValue(name)?.ToString()?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 内存状态
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryStatusEx
        {
            /// <summary>
            /// 结构体大小，调用前必须赋值
            /// </summary>
            public uint Length;

            /// <summary>
            /// 内存使用率
            /// </summary>
            public uint MemoryLoad;

            /// <summary>
            /// 物理内存总量
            /// </summary>
            public ulong TotalPhys;

            /// <summary>
            /// 可用物理内存
            /// </summary>
            public ulong AvailPhys;

            /// <summary>
            /// 页面文件总量
            /// </summary>
            public ulong TotalPageFile;

            /// <summary>
            /// 可用页面文件
            /// </summary>
            public ulong AvailPageFile;

            /// <summary>
            /// 虚拟内存总量
            /// </summary>
            public ulong TotalVirtual;

            /// <summary>
            /// 可用虚拟内存
            /// </summary>
            public ulong AvailVirtual;

            /// <summary>
            /// 扩展虚拟内存
            /// </summary>
            public ulong AvailExtendedVirtual;
        }

        /// <summary>
        /// GetLogicalProcessorInformation 返回的处理器关系条目
        /// </summary>
        /// <remarks>
        /// 末尾两个 ulong 对应原结构体里的 union（PROCESSOR_CACHE_DESCRIPTOR 为 16 字节），
        /// x64 下整体 32 字节、x86 下 24 字节，与原生结构一致。
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct SystemLogicalProcessorInformation
        {
            /// <summary>
            /// 处理器掩码
            /// </summary>
            public UIntPtr ProcessorMask;

            /// <summary>
            /// 关系类型，0 表示物理核心
            /// </summary>
            public int Relationship;

            /// <summary>
            /// 缓存/标志位第一段
            /// </summary>
            public ulong Reserved0;

            /// <summary>
            /// 缓存/标志位第二段
            /// </summary>
            public ulong Reserved1;
        }

        /// <summary>
        /// 获取系统内存状态
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

        /// <summary>
        /// 获取系统累计的 idle / kernel / user 时间
        /// </summary>
        /// <param name="idleTime"></param>
        /// <param name="kernelTime"></param>
        /// <param name="userTime"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSystemTimes(out ulong idleTime, out ulong kernelTime, out ulong userTime);

        /// <summary>
        /// 获取逻辑处理器与物理核心的对应关系
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="returnedLength"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLogicalProcessorInformation(IntPtr buffer, ref uint returnedLength);

        /// <summary>
        /// 按关系类型获取处理器拓扑（支持物理 CPU 封装）
        /// </summary>
        /// <param name="relationshipType"></param>
        /// <param name="buffer"></param>
        /// <param name="returnedLength"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLogicalProcessorInformationEx(
            int relationshipType, IntPtr buffer, ref uint returnedLength);
    }

    /// <summary>
    /// Linux 系统
    /// </summary>
    internal static class PlatformForLinux
    {
        /// <summary>
        /// 两次采样 /proc/stat 的间隔，用于计算瞬时 CPU 利用率
        /// </summary>
        private const int CpuSampleIntervalMs = 100;

        /// <summary>
        /// 内存信息路径
        /// </summary>
        private const string MemInfoPath = "/proc/meminfo";

        /// <summary>
        /// 处理器信息路径
        /// </summary>
        private const string CpuInfoPath = "/proc/cpuinfo";

        /// <summary>
        /// 处理器时间片路径
        /// </summary>
        private const string StatPath = "/proc/stat";

        /// <summary>
        /// 采集 Linux 指标
        /// </summary>
        /// <param name="os"></param>
        public static void Fill(OsInfo os)
        {
            var memory = ReadMemInfo();

            os.TotalPhysicalMemory = memory.GetValueOrDefault("MemTotal");
            os.FreePhysicalMemory = memory.TryGetValue("MemAvailable", out var available)
                ? available
                // 3.14 之前的内核没有 MemAvailable，按 free + buffers + cached 估算
                : memory.GetValueOrDefault("MemFree") + memory.GetValueOrDefault("Buffers") +
                  memory.GetValueOrDefault("Cached");
            os.SwapTotal = memory.GetValueOrDefault("SwapTotal");
            os.SwapFree = memory.GetValueOrDefault("SwapFree");

            var cpu = ReadCpuInfo();

            os.ProcessorName = cpu.ModelName;
            os.PhysicalProcessorCount = cpu.PhysicalPackages > 0 ? cpu.PhysicalPackages : 1;
            os.LogicProcessorCount = cpu.LogicalProcessors > 0 ? cpu.LogicalProcessors : Environment.ProcessorCount;
            os.NumberOfCores = cpu.PhysicalCores > 0 ? cpu.PhysicalCores : os.LogicProcessorCount;
            os.ProcessorUtilizationRate = ReadCpuUsage();
        }

        /// <summary>
        /// 读取 /proc/meminfo，键值统一换算成字节
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, long> ReadMemInfo(string path = MemInfoPath)
        {
            var values = new Dictionary<string, long>(StringComparer.Ordinal);

            try
            {
                foreach (var line in File.ReadLines(path))
                {
                    var colon = line.IndexOf(':');
                    if (colon <= 0)
                    {
                        continue;
                    }

                    var key = line[..colon].Trim();
                    var rest = line[(colon + 1)..].Trim();
                    var space = rest.IndexOf(' ');
                    var number = space > 0 ? rest[..space] : rest;

                    if (long.TryParse(number, out var kb))
                    {
                        values[key] = kb * 1024;
                    }
                }
            }
            catch
            {
                // 容器里 /proc 被裁剪或不可读时返回空表
            }

            return values;
        }

        /// <summary>
        /// 读取 /proc/cpuinfo
        /// </summary>
        /// <returns></returns>
        private static CpuSnapshot ReadCpuInfo(string path = CpuInfoPath)
        {
            var snapshot = new CpuSnapshot();
            var packages = new HashSet<string>(StringComparer.Ordinal);
            var cores = new HashSet<string>(StringComparer.Ordinal);
            var currentPackage = "0";
            var logical = 0;

            try
            {
                foreach (var line in File.ReadLines(path))
                {
                    var colon = line.IndexOf(':');
                    if (colon <= 0)
                    {
                        continue;
                    }

                    var key = line[..colon].Trim();
                    var value = line[(colon + 1)..].Trim();

                    switch (key)
                    {
                        case "processor":
                            logical++;
                            break;
                        case "model name":
                            if (string.IsNullOrEmpty(snapshot.ModelName))
                            {
                                snapshot.ModelName = value;
                            }

                            break;
                        case "physical id":
                            currentPackage = value;
                            packages.Add(value);
                            break;
                        case "core id":
                            // core id 只在同一个物理 CPU 内唯一，需与 physical id 组合去重
                            cores.Add($"{currentPackage}:{value}");
                            break;
                    }
                }
            }
            catch
            {
                // /proc/cpuinfo 不可读时用下面的兜底值
            }

            snapshot.LogicalProcessors = logical;
            snapshot.PhysicalPackages = packages.Count;
            snapshot.PhysicalCores = cores.Count;

            return snapshot;
        }

        /// <summary>
        /// 通过两次采样 /proc/stat 计算 CPU 利用率
        /// </summary>
        /// <returns></returns>
        private static int ReadCpuUsage(string path = StatPath)
        {
            try
            {
                var first = ReadCpuTimes(path);
                if (first is null)
                {
                    return 0;
                }

                Thread.Sleep(CpuSampleIntervalMs);

                var second = ReadCpuTimes(path);
                if (second is null)
                {
                    return 0;
                }

                var totalDelta = second.Value.Total - first.Value.Total;
                var idleDelta = second.Value.Idle - first.Value.Idle;

                if (totalDelta <= 0)
                {
                    return 0;
                }

                var usage = (int)Math.Round((totalDelta - idleDelta) * 100.0 / totalDelta);
                return Math.Clamp(usage, 0, 100);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 读取 /proc/stat 中 CPU 累计时间片
        /// </summary>
        /// <returns></returns>
        private static (long Total, long Idle)? ReadCpuTimes(string path)
        {
            try
            {
                var line = File.ReadLines(path)
                    .FirstOrDefault(l => l.StartsWith("cpu ", StringComparison.Ordinal));

                if (line is null)
                {
                    return null;
                }

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                {
                    return null;
                }

                long total = 0;
                for (var i = 1; i < parts.Length; i++)
                {
                    if (!long.TryParse(parts[i], out var value))
                    {
                        return null;
                    }

                    total += value;
                }

                // user nice system idle iowait ...，idle 与 iowait 都算空闲
                var idle = long.Parse(parts[4]) + (parts.Length > 5 ? long.Parse(parts[5]) : 0);

                return (total, idle);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// /proc/cpuinfo 的解析结果
        /// </summary>
        private struct CpuSnapshot
        {
            /// <summary>
            /// 处理器型号
            /// </summary>
            public string ModelName;

            /// <summary>
            /// 物理 CPU 数量
            /// </summary>
            public int PhysicalPackages;

            /// <summary>
            /// 物理核心数量
            /// </summary>
            public int PhysicalCores;

            /// <summary>
            /// 逻辑处理器数量
            /// </summary>
            public int LogicalProcessors;
        }
    }

    /// <summary>
    /// macOS 等其它类 Unix 系统
    /// </summary>
    internal static class PlatformForUnix
    {
        /// <summary>
        /// 采集指标
        /// </summary>
        /// <param name="os"></param>
        public static void Fill(OsInfo os)
        {
            os.LogicProcessorCount = Environment.ProcessorCount;
            os.NumberOfCores = Environment.ProcessorCount;
            os.PhysicalProcessorCount = 1;
            os.ProcessorName = ReadSysctl("machdep.cpu.brand_string");

            if (long.TryParse(ReadSysctl("hw.memsize"), out var totalMemory))
            {
                os.TotalPhysicalMemory = totalMemory;
            }

            os.FreePhysicalMemory = ReadFreeMemory();
        }

        /// <summary>
        /// 读取 sysctl 值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string ReadSysctl(string key)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo("sysctl", $"-n {key}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process is null)
                {
                    return string.Empty;
                }

                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(2000);

                return output;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 通过 vm_stat 估算可用内存
        /// </summary>
        /// <returns></returns>
        private static long ReadFreeMemory()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo("vm_stat")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process is null)
                {
                    return 0;
                }

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);

                long pageSize = 4096;
                var pageSizeMatch = Regex.Match(
                    output, @"page size of (\d+) bytes");
                if (pageSizeMatch.Success)
                {
                    pageSize = long.Parse(pageSizeMatch.Groups[1].Value);
                }

                long pages = 0;
                foreach (var key in new[] { "Pages free", "Pages inactive", "Pages speculative" })
                {
                    var match = Regex.Match(
                        output, $@"{key}:\s+(\d+)\.");
                    if (match.Success)
                    {
                        pages += long.Parse(match.Groups[1].Value);
                    }
                }

                return pages * pageSize;
            }
            catch
            {
                return 0;
            }
        }
    }
}

/// <summary>
/// 磁盘信息
/// </summary>
public class DiskInfo
{
    /// <summary>
    ///  名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    ///  总大小 
    /// </summary>
    public long Size { get; set; }
    /// <summary>
    ///  可用空间 
    /// </summary>
    public long FreeSpace { get; set; }
}
