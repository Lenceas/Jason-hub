using System.Diagnostics;
using MonitorApi.Models.Entities;
using MonitorApi.Services;

namespace MonitorApi.Worker.Collectors;

/// <summary>站点可用性探测采集器 — 定时 HTTP GET 检查各监控站点</summary>
public class SitePoller
{
    private readonly ILogger<SitePoller> _logger;
    private readonly HttpClient _httpClient;

    public SitePoller(ILogger<SitePoller> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("SitePoller");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Monitor-Agent/1.0");
    }

    /// <summary>探测所有活跃站点</summary>
    /// <param name="svc">Monitor 业务服务</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task PollAllAsync(MonitorService svc, CancellationToken cancellationToken)
    {
        try
        {
            var sites = await svc.GetSitesAsync();
            var activeSites = sites.Where(s => s.IsActive).ToList();

            _logger.LogDebug("开始探测 {Count} 个活跃站点", activeSites.Count);

            foreach (var site in activeSites)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await PollSiteAsync(svc, site);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "站点批量探测失败");
        }
    }

    /// <summary>探测单个站点</summary>
    /// <param name="svc">Monitor 业务服务</param>
    /// <param name="site">站点信息</param>
    private async Task PollSiteAsync(MonitorService svc, Models.SiteResponse site)
    {
        var record = new MonitorUptimeRecord
        {
            SiteId = site.Id,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(site.TimeoutMs));
            var sw = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync(site.Url, cts.Token);
            sw.Stop();

            record.StatusCode = (int)response.StatusCode;
            record.ResponseMs = (int)sw.ElapsedMilliseconds;
            record.IsOk = response.IsSuccessStatusCode || (int)response.StatusCode < 500;

            _logger.LogDebug(
                "站点 {Name} ({Url}): {StatusCode} {ResponseMs}ms",
                site.Name, site.Url, record.StatusCode, record.ResponseMs);
        }
        catch (TaskCanceledException)
        {
            record.IsOk = false;
            record.StatusCode = 0;
            record.ResponseMs = site.TimeoutMs;
            _logger.LogWarning("站点 {Name} ({Url}) 超时", site.Name, site.Url);
        }
        catch (HttpRequestException ex)
        {
            record.IsOk = false;
            record.StatusCode = 0;
            record.ResponseMs = null;
            _logger.LogWarning(ex, "站点 {Name} ({Url}) 请求失败", site.Name, site.Url);
        }

        await svc.InsertUptimeRecordAsync(record);
    }
}
