using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text.Json;
using MonitorApi.Models.Entities;

namespace MonitorApi.Worker.Collectors;

/// <summary>服务器指标采集器 — 从系统 API 和 /proc 文件系统采集指标，支持 Windows/Linux</summary>
public partial class ServerMetricsCollector : IDisposable
{
    private readonly ILogger<ServerMetricsCollector> _logger;

    // Linux CPU delta 跟踪
    private long _prevIdle;
    private long _prevTotal;

    // Windows CPU 计数器
    private PerformanceCounter? _cpuCounter;
    private bool _cpuWarmed;

    public ServerMetricsCollector(ILogger<ServerMetricsCollector> logger)
    {
        _logger = logger;
    }

    /// <summary>采集一次服务器指标</summary>
    /// <returns>指标实体，采集失败返回 null</returns>
    public async Task<MonitorServerMetric?> CollectAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return await CollectLinuxAsync();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return await CollectWindowsAsync();

            _logger.LogWarning("不支持的平台: {OS}", RuntimeInformation.OSDescription);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "服务器指标采集失败");
            return null;
        }
    }

    // ======== Linux 采集 ========

    private async Task<MonitorServerMetric?> CollectLinuxAsync()
    {
        var disk = GetDiskUsage();
        var metric = new MonitorServerMetric
        {
            Ts = DateTime.UtcNow,
            CpuPct = await GetCpuUsageLinuxAsync(),
            MemPct = GetMemoryUsageLinux(),
            MemUsed = GetMemoryUsedLinux(),
            MemTotal = GetMemoryTotalLinux(),
            DiskPct = disk.pct,
            DiskUsed = disk.used,
            DiskTotal = disk.total,
            NetIn = GetNetworkStatsLinux().inBytes,
            NetOut = GetNetworkStatsLinux().outBytes,
            Load1m = GetLoadAverageLinux().load1,
            Load5m = GetLoadAverageLinux().load5,
            Load15m = GetLoadAverageLinux().load15
        };

        await Task.CompletedTask;
        return metric;
    }

    // ======== Windows 采集 ========

    private async Task<MonitorServerMetric?> CollectWindowsAsync()
    {
        var disk = GetDiskUsage();
        var memory = GetMemoryWindows();
        var network = GetNetworkStatsWindows();

        var metric = new MonitorServerMetric
        {
            Ts = DateTime.UtcNow,
            CpuPct = await GetCpuUsageWindowsAsync(),
            MemPct = memory.usagePct,
            MemUsed = memory.used,
            MemTotal = memory.total,
            DiskPct = disk.pct,
            DiskUsed = disk.used,
            DiskTotal = disk.total,
            NetIn = network.inBytes,
            NetOut = network.outBytes,
            Load1m = null,
            Load5m = null,
            Load15m = null
        };

        return metric;
    }

    // ==================================================================
    //  CPU
    // ==================================================================

    /// <summary>Linux CPU — 读取 /proc/stat 计算使用率</summary>
    private async Task<decimal?> GetCpuUsageLinuxAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return null;

        try
        {
            var stat = await File.ReadAllTextAsync("/proc/stat");
            var match = CpuLineRegex().Match(stat);
            if (!match.Success) return null;

            var user = long.Parse(match.Groups[1].Value);
            var nice = long.Parse(match.Groups[2].Value);
            var system = long.Parse(match.Groups[3].Value);
            var idle = long.Parse(match.Groups[4].Value);
            var iowait = long.Parse(match.Groups[5].Value);
            var irq = long.Parse(match.Groups[6].Value);
            var softirq = long.Parse(match.Groups[7].Value);
            var steal = long.Parse(match.Groups[8].Value);

            var idleDelta = idle - _prevIdle;
            var total = user + nice + system + idle + iowait + irq + softirq + steal;
            var totalDelta = total - _prevTotal;

            _prevIdle = idle;
            _prevTotal = total;

            if (_prevTotal == 0 || totalDelta == 0) return null;

            return (decimal?)Math.Round((1 - (double)idleDelta / totalDelta) * 100, 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Linux CPU 采集失败");
            return null;
        }
    }

    /// <summary>Windows CPU — 使用 PerformanceCounter</summary>
    /// <remarks>首次返回 null（预热），后续每 10s 采集一次周期平均</remarks>
    private async Task<decimal?> GetCpuUsageWindowsAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            _cpuCounter ??= new PerformanceCounter("Processor", "% Processor Time", "_Total");

            if (!_cpuWarmed)
            {
                _cpuCounter.NextValue(); // 首次采样返回 0，丢弃
                _cpuWarmed = true;
                return null;
            }

            await Task.CompletedTask;
            return (decimal?)Math.Round(_cpuCounter.NextValue(), 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Windows CPU 采集失败");
            return null;
        }
    }

    // ==================================================================
    //  内存
    // ==================================================================

    /// <summary>Linux 内存 — 读取 /proc/meminfo</summary>
    private static decimal? GetMemoryUsageLinux()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return null;

        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            var total = ParseMemInfo(lines, "MemTotal");
            var available = ParseMemInfo(lines, "MemAvailable");
            if (total <= 0) return null;
            return (decimal?)Math.Round((double)(total - available) / total * 100, 2);
        }
        catch { return null; }
    }

    private static long? GetMemoryUsedLinux()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return null;

        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            var total = ParseMemInfo(lines, "MemTotal");
            var available = ParseMemInfo(lines, "MemAvailable");
            return (total - available) * 1024;
        }
        catch { return null; }
    }

    private static long? GetMemoryTotalLinux()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return null;

        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            return ParseMemInfo(lines, "MemTotal") * 1024;
        }
        catch { return null; }
    }

    /// <summary>Windows 内存 — 使用 Kernel32.GlobalMemoryStatusEx</summary>
    private static (long? total, long? used, decimal? usagePct) GetMemoryWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return (null, null, null);

        try
        {
            var status = new MEMORYSTATUSEX
            {
                dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
            };
            if (!GlobalMemoryStatusEx(ref status))
                return (null, null, null);

            var total = (long)status.ullTotalPhys;
            var available = (long)status.ullAvailPhys;
            var used = total - available;
            var pct = total > 0
                ? (decimal?)Math.Round((double)used / total * 100, 2)
                : null;

            return (total, used, pct);
        }
        catch
        {
            return (null, null, null);
        }
    }

    private static long ParseMemInfo(string[] lines, string key)
    {
        foreach (var line in lines)
        {
            if (line.StartsWith($"{key}:"))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && long.TryParse(parts[1], out var val))
                    return val;
            }
        }
        return 0;
    }

    // ==================================================================
    //  磁盘（跨平台 — DriveInfo）
    // ==================================================================

    private static (decimal? pct, long? used, long? total) GetDiskUsage()
    {
        try
        {
            var rootDrive = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.Name == "/" || d.Name == "/rootfs"
                    || (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && d.Name.StartsWith("C:")));
            if (rootDrive == null || !rootDrive.IsReady) return (null, null, null);
            var total = rootDrive.TotalSize;
            var free = rootDrive.TotalFreeSpace;
            if (total <= 0) return (null, null, null);
            var used = total - free;
            var pct = (decimal?)Math.Round((double)used / total * 100, 2);
            return (pct, used, total);
        }
        catch { return (null, null, null); }
    }

    // ==================================================================
    //  网络
    // ==================================================================

    /// <summary>Linux 网络 — 读取 /proc/net/dev</summary>
    private static (long inBytes, long outBytes) GetNetworkStatsLinux()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return (0, 0);

        try
        {
            var text = File.ReadAllText("/proc/net/dev");
            long totalIn = 0, totalOut = 0;
            foreach (var line in text.Split('\n'))
            {
                if (!line.Contains(':') || line.TrimStart().StartsWith("lo:"))
                    continue;

                var parts = NetworkLineRegex().Split(line.Trim());
                if (parts.Length >= 10)
                {
                    totalIn += long.TryParse(parts[1], out var b) ? b : 0;
                    totalOut += long.TryParse(parts[9], out var b2) ? b2 : 0;
                }
            }
            return (totalIn, totalOut);
        }
        catch { return (0, 0); }
    }

    /// <summary>Windows 网络 — 使用 NetworkInterface API</summary>
    private static (long inBytes, long outBytes) GetNetworkStatsWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return (0, 0);

        try
        {
            long totalIn = 0, totalOut = 0;
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var stats = ni.GetIPv4Statistics();
                totalIn += stats.BytesReceived;
                totalOut += stats.BytesSent;
            }
            return (totalIn, totalOut);
        }
        catch { return (0, 0); }
    }

    // ==================================================================
    //  系统负载（仅 Linux）
    // ==================================================================

    private static (decimal? load1, decimal? load5, decimal? load15) GetLoadAverageLinux()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return (null, null, null);

        try
        {
            var text = File.ReadAllText("/proc/loadavg");
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                return (
                    decimal.TryParse(parts[0], out var l1) ? l1 : null,
                    decimal.TryParse(parts[1], out var l5) ? l5 : null,
                    decimal.TryParse(parts[2], out var l15) ? l15 : null
                );
            }
            return (null, null, null);
        }
        catch { return (null, null, null); }
    }

    // ==================================================================
    //  P/Invoke — Windows 内存查询
    // ==================================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    // ==================================================================
    //  Regex
    // ==================================================================

    [GeneratedRegex(@"^cpu\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)")]
    private static partial Regex CpuLineRegex();

    [GeneratedRegex(@"[:\s]+")]
    private static partial Regex NetworkLineRegex();

    public void Dispose()
    {
        _cpuCounter?.Dispose();
        _cpuCounter = null;
    }
}
