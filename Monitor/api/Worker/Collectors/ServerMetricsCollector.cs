using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MonitorApi.Models.Entities;

namespace MonitorApi.Worker.Collectors;

/// <summary>服务器指标采集器 — 从 /proc 文件系统和系统 API 采集指标</summary>
public partial class ServerMetricsCollector
{
    private readonly ILogger<ServerMetricsCollector> _logger;

    // CPU 计算用：上次采样值
    private long _prevIdle;
    private long _prevTotal;

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
            var metric = new MonitorServerMetric
            {
                Ts = DateTime.UtcNow,
                CpuPct = await GetCpuUsageAsync(),
                MemPct = GetMemoryUsage(),
                MemUsed = GetMemoryUsed(),
                MemTotal = GetMemoryTotal(),
                DiskPct = GetDiskUsage(),
                NetIn = GetNetworkStats().inBytes,
                NetOut = GetNetworkStats().outBytes,
                Load1m = GetLoadAverage().load1,
                Load5m = GetLoadAverage().load5,
                Load15m = GetLoadAverage().load15
            };

            await Task.CompletedTask;
            return metric;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "服务器指标采集失败");
            return null;
        }
    }

    // ======== CPU ========

    private async Task<decimal?> GetCpuUsageAsync()
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

            if (_prevTotal == 0 || totalDelta == 0) return null; // 首次采样

            return (decimal?)Math.Round((1 - (double)idleDelta / totalDelta) * 100, 2);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CPU 采集失败");
            return null;
        }
    }

    // ======== 内存 ========

    private static decimal? GetMemoryUsage()
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

    private static long? GetMemoryUsed()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return null;

        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            var total = ParseMemInfo(lines, "MemTotal");
            var available = ParseMemInfo(lines, "MemAvailable");
            return (total - available) * 1024; // kB → bytes
        }
        catch { return null; }
    }

    private static long? GetMemoryTotal()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return null;

        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            return ParseMemInfo(lines, "MemTotal") * 1024; // kB → bytes
        }
        catch { return null; }
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

    // ======== 磁盘 ========

    private static decimal? GetDiskUsage()
    {
        try
        {
            // 取根分区使用率
            var rootDrive = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.Name == "/" || d.Name == "/rootfs");
            if (rootDrive == null || !rootDrive.IsReady) return null;
            var total = rootDrive.TotalSize;
            var free = rootDrive.TotalFreeSpace;
            if (total <= 0) return null;
            return (decimal?)Math.Round((double)(total - free) / total * 100, 2);
        }
        catch { return null; }
    }

    // ======== 网络 ========

    private static (long inBytes, long outBytes) GetNetworkStats()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return (0, 0);

        try
        {
            var text = File.ReadAllText("/proc/net/dev");
            long totalIn = 0, totalOut = 0;
            foreach (var line in text.Split('\n'))
            {
                // 跳过标题行和 lo 回环接口
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

    // ======== 系统负载 ========

    private static (decimal? load1, decimal? load5, decimal? load15) GetLoadAverage()
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

    [GeneratedRegex(@"^cpu\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)\s+(\d+)")]
    private static partial Regex CpuLineRegex();

    [GeneratedRegex(@"[:\s]+")]
    private static partial Regex NetworkLineRegex();
}
