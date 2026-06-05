using System.Diagnostics;
using System.Runtime.InteropServices;
using MonitorApi.Models.Entities;
using MonitorApi.Services;

namespace MonitorApi.Worker.Collectors;

/// <summary>应用健康检查采集器 — 定时调用各服务健康端点</summary>
public class HealthCheckCollector
{
    private readonly ILogger<HealthCheckCollector> _logger;
    private readonly HttpClient _httpClient;

    // 被监控的服务列表（Docker Compose 网络内使用容器端口）
    private static readonly List<(string name, string url)> LinuxServices =
    [
        ("auth",      "http://auth:8100/healthz"),
        ("monitor",   "http://localhost:8051/healthz")
        // TODO: notification 上线后追加
        // ("notification", "http://notification:8110/healthz"),
    ];

    // Windows 本地开发 — 只检查本地可达的服务
    private static readonly List<(string name, string url)> WindowsServices =
    [
        ("monitor",   "http://localhost:8051/healthz")
    ];

    private List<(string name, string url)> Services => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? WindowsServices
        : LinuxServices;

    public HealthCheckCollector(ILogger<HealthCheckCollector> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("HealthCheck");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Monitor-Agent/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

    /// <summary>检查所有服务健康状态</summary>
    /// <param name="svc">Monitor 业务服务</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task CheckAllAsync(MonitorService svc, CancellationToken cancellationToken)
    {
        foreach (var (name, url) in Services)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await CheckServiceAsync(svc, name, url);
        }
    }

    /// <summary>检查单个服务</summary>
    /// <param name="svc">Monitor 业务服务</param>
    /// <param name="name">服务名称</param>
    /// <param name="url">健康检查端点 URL</param>
    private async Task CheckServiceAsync(MonitorService svc, string name, string url)
    {
        var record = new MonitorHealthRecord
        {
            Service = name,
            Endpoint = url,
            Ts = DateTime.UtcNow
        };

        try
        {
            var sw = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync(url);
            sw.Stop();

            record.Status = $"{(int)response.StatusCode} {(response.IsSuccessStatusCode ? "ok" : "fail")}";
            record.LatencyMs = (int)sw.ElapsedMilliseconds;

            _logger.LogDebug("健康检查 {Name}: {Status} {LatencyMs}ms", name, record.Status, record.LatencyMs);
        }
        catch (Exception ex)
        {
            record.Status = "unreachable";
            record.LatencyMs = null;
            _logger.LogWarning(ex, "健康检查 {Name} 不可达（{Url}）", name, url);
        }

        await svc.InsertHealthRecordAsync(record);
    }
}
