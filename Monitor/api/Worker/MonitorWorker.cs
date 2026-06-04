using MonitorApi.Services;
using MonitorApi.Worker.Collectors;

namespace MonitorApi.Worker;

/// <summary>Monitor 后台采集 Worker — 按固定间隔采集服务器/Docker/站点/健康等指标</summary>
public class MonitorWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitorWorker> _logger;

    // 采集间隔（毫秒）
    private const int MetricsIntervalMs = 10_000;     // 服务器指标：10s
    private const int UptimeIntervalMs = 60_000;      // 站点探测：60s
    private const int HealthIntervalMs = 30_000;      // 健康检查：30s
    private const int DockerIntervalMs = 30_000;      // Docker：30s（预留）

    public MonitorWorker(IServiceScopeFactory scopeFactory, ILogger<MonitorWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitor Worker 启动");

        var metricsDue = DateTime.UtcNow;
        var uptimeDue = DateTime.UtcNow;
        var healthDue = DateTime.UtcNow;
        var dockerDue = DateTime.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // 服务器指标（10s）
                if (now >= metricsDue)
                {
                    metricsDue = now.AddMilliseconds(MetricsIntervalMs);
                    await CollectServerMetricsAsync(stoppingToken);
                }

                // 站点探测（60s）
                if (now >= uptimeDue)
                {
                    uptimeDue = now.AddMilliseconds(UptimeIntervalMs);
                    await PollSitesAsync(stoppingToken);
                }

                // 健康检查（30s）
                if (now >= healthDue)
                {
                    healthDue = now.AddMilliseconds(HealthIntervalMs);
                    await CheckHealthAsync(stoppingToken);
                }

                // Docker 容器（30s, 预留）
                if (now >= dockerDue)
                {
                    dockerDue = now.AddMilliseconds(DockerIntervalMs);
                    await CollectDockerAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker 主循环异常");
            }

            // 每 5 秒检查一次
            await Task.Delay(5000, stoppingToken);
        }

        _logger.LogInformation("Monitor Worker 停止");
    }

    /// <summary>采集服务器指标</summary>
    /// <param name="ct">取消令牌</param>
    private async Task CollectServerMetricsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var collector = scope.ServiceProvider.GetRequiredService<ServerMetricsCollector>();
        var svc = scope.ServiceProvider.GetRequiredService<MonitorService>();

        var metric = await collector.CollectAsync();
        if (metric is not null)
        {
            await svc.InsertMetricAsync(metric);
            _logger.LogDebug("服务器指标已采集: CPU={Cpu}% MEM={Mem}% DISK={Disk}%",
                metric.CpuPct, metric.MemPct, metric.DiskPct);
        }
    }

    /// <summary>探测站点可用性</summary>
    /// <param name="ct">取消令牌</param>
    private async Task PollSitesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var poller = scope.ServiceProvider.GetRequiredService<SitePoller>();
        var svc = scope.ServiceProvider.GetRequiredService<MonitorService>();

        await poller.PollAllAsync(svc, ct);
    }

    /// <summary>检查服务健康状态</summary>
    /// <param name="ct">取消令牌</param>
    private async Task CheckHealthAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var checker = scope.ServiceProvider.GetRequiredService<HealthCheckCollector>();
        var svc = scope.ServiceProvider.GetRequiredService<MonitorService>();

        await checker.CheckAllAsync(svc, ct);
    }

    /// <summary>采集 Docker 容器状态（预留）</summary>
    /// <param name="ct">取消令牌</param>
    private Task CollectDockerAsync(CancellationToken ct)
    {
        // TODO: 集成 Docker.DotNet SDK 后实现
        return Task.CompletedTask;
    }
}
