using MonitorApi.Models;
using MonitorApi.Models.Entities;
using SqlSugar;

namespace MonitorApi.Services;

/// <summary>Monitor 核心业务服务</summary>
public class MonitorService
{
    private readonly ISqlSugarClient _db;

    public MonitorService(ISqlSugarClient db)
    {
        _db = db;
    }

    // ======== 服务器指标 ========

    /// <summary>获取最新的服务器指标</summary>
    /// <returns>最新一条指标记录，无数据则返回 null</returns>
    public async Task<ServerMetricsResponse?> GetLatestMetricsAsync()
    {
        var record = await _db.Queryable<MonitorServerMetric>()
            .OrderByDescending(m => m.Ts)
            .FirstAsync();
        return record is null ? null : MapMetric(record);
    }

    /// <summary>获取历史指标</summary>
    /// <param name="rangeHours">查询范围（小时），默认 24h，范围 1-168h</param>
    /// <returns>指定时间范围内的指标记录列表（按时间正序）</returns>
    public async Task<List<ServerMetricsResponse>> GetMetricsHistoryAsync(int rangeHours = 24)
    {
        var since = DateTime.UtcNow.AddHours(-rangeHours);
        var records = await _db.Queryable<MonitorServerMetric>()
            .Where(m => m.Ts >= since)
            .OrderBy(m => m.Ts)
            .ToListAsync();
        return records.Select(MapMetric).ToList();
    }

    /// <summary>写入一条服务器指标（由 Worker 采集器调用）</summary>
    /// <param name="metric">服务器指标实体</param>
    public async Task InsertMetricAsync(MonitorServerMetric metric)
    {
        await _db.Insertable(metric).ExecuteCommandAsync();
    }

    // ======== 站点监控 ========

    /// <summary>获取所有监控站点</summary>
    /// <returns>站点列表</returns>
    public async Task<List<SiteResponse>> GetSitesAsync()
    {
        var sites = await _db.Queryable<MonitorSite>().ToListAsync();
        return sites.Select(MapSite).ToList();
    }

    /// <summary>新增监控站点</summary>
    /// <param name="req">站点创建请求（名称、URL、探测间隔等）</param>
    /// <returns>创建后的站点信息（含自动生成的 ID）</returns>
    public async Task<SiteResponse> CreateSiteAsync(SiteRequest req)
    {
        var site = new MonitorSite
        {
            Name = req.Name,
            Url = req.Url,
            IntervalSec = req.IntervalSec ?? 60,
            TimeoutMs = req.TimeoutMs ?? 5000
        };
        var id = await _db.Insertable(site).ExecuteReturnIdentityAsync();
        site.Id = id;
        return MapSite(site);
    }

    /// <summary>删除监控站点</summary>
    /// <param name="id">站点 ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteSiteAsync(int id)
    {
        var rows = await _db.Deleteable<MonitorSite>(id).ExecuteCommandAsync();
        return rows > 0;
    }

    // ======== 站点可用性 ========

    /// <summary>获取站点 24h 可用性统计</summary>
    /// <param name="siteId">站点 ID</param>
    /// <returns>可用率统计 + 检查记录列表，站点不存在则返回 null</returns>
    public async Task<UptimeHistoryResponse?> GetSiteUptimeAsync(int siteId)
    {
        var site = await _db.Queryable<MonitorSite>().FirstAsync(s => s.Id == siteId);
        if (site is null) return null;

        var since = DateTime.UtcNow.AddHours(-24);
        var records = await _db.Queryable<MonitorUptimeRecord>()
            .Where(r => r.SiteId == siteId && r.CheckedAt >= since)
            .OrderByDescending(r => r.CheckedAt)
            .ToListAsync();

        var total = records.Count;
        var ok = records.Count(r => r.IsOk);
        var uptimePct = total > 0 ? Math.Round((decimal)ok / total * 100, 2) : 0;

        return new UptimeHistoryResponse(
            SiteId: siteId,
            UptimePct: uptimePct,
            TotalChecks: total,
            OkChecks: ok,
            Records: records.Select(r => new UptimeRecordResponse(
                Id: r.Id, StatusCode: r.StatusCode, ResponseMs: r.ResponseMs,
                IsOk: r.IsOk, CheckedAt: r.CheckedAt
            )).ToList()
        );
    }

    /// <summary>写入一条站点可用性检查记录（由 Worker 采集器调用）</summary>
    /// <param name="record">可用性检查记录实体</param>
    public async Task InsertUptimeRecordAsync(MonitorUptimeRecord record)
    {
        await _db.Insertable(record).ExecuteCommandAsync();
    }

    // ======== 容器快照 ========

    /// <summary>获取最新一次采集的容器快照列表</summary>
    /// <returns>所有容器的最新状态快照</returns>
    public async Task<List<ContainerSnapshotResponse>> GetLatestContainerSnapshotsAsync()
    {
        var latestTs = await _db.Queryable<MonitorContainerSnapshot>()
            .MaxAsync(s => (DateTime?)s.Ts);
        if (latestTs is null) return new();

        var snapshots = await _db.Queryable<MonitorContainerSnapshot>()
            .Where(s => s.Ts == latestTs)
            .ToListAsync();
        return snapshots.Select(s => new ContainerSnapshotResponse(
            Name: s.Name, Status: s.Status, CpuPct: s.CpuPct,
            MemUsage: s.MemUsage, MemLimit: s.MemLimit, Ts: s.Ts
        )).ToList();
    }

    /// <summary>批量写入容器快照（由 Worker 采集器调用）</summary>
    /// <param name="snapshots">容器快照列表</param>
    public async Task InsertContainerSnapshotsAsync(List<MonitorContainerSnapshot> snapshots)
    {
        await _db.Insertable(snapshots).ExecuteCommandAsync();
    }

    // ======== 健康检查 ========

    /// <summary>获取所有服务的最新健康检查状态</summary>
    /// <returns>每个服务的最新健康记录</returns>
    public async Task<List<HealthRecordResponse>> GetLatestHealthRecordsAsync()
    {
        var services = await _db.Queryable<MonitorHealthRecord>()
            .Select(r => r.Service)
            .Distinct()
            .ToListAsync();

        var result = new List<HealthRecordResponse>();
        foreach (var service in services)
        {
            var latest = await _db.Queryable<MonitorHealthRecord>()
                .Where(r => r.Service == service)
                .OrderByDescending(r => r.Ts)
                .FirstAsync();
            if (latest is not null)
                result.Add(new HealthRecordResponse(
                    Service: latest.Service, Status: latest.Status,
                    LatencyMs: latest.LatencyMs, Ts: latest.Ts
                ));
        }
        return result;
    }

    /// <summary>写入一条健康检查记录（由 Worker 采集器调用）</summary>
    /// <param name="record">健康检查记录实体</param>
    public async Task InsertHealthRecordAsync(MonitorHealthRecord record)
    {
        await _db.Insertable(record).ExecuteCommandAsync();
    }

    // ======== 告警规则 ========

    /// <summary>获取所有告警规则</summary>
    /// <returns>告警规则列表</returns>
    public async Task<List<AlertRuleResponse>> GetAlertRulesAsync()
    {
        var rules = await _db.Queryable<MonitorAlertRule>().ToListAsync();
        return rules.Select(MapAlertRule).ToList();
    }

    /// <summary>创建告警规则</summary>
    /// <param name="req">告警规则创建请求</param>
    /// <returns>创建后的告警规则（含自动生成的 ID）</returns>
    public async Task<AlertRuleResponse> CreateAlertRuleAsync(AlertRuleRequest req)
    {
        var rule = new MonitorAlertRule
        {
            Name = req.Name,
            Metric = req.Metric,
            Operator = req.Operator,
            Threshold = req.Threshold,
            DurationSec = req.DurationSec ?? 300
        };
        var id = await _db.Insertable(rule).ExecuteReturnIdentityAsync();
        rule.Id = id;
        return MapAlertRule(rule);
    }

    /// <summary>更新告警规则</summary>
    /// <param name="id">规则 ID</param>
    /// <param name="req">新的规则配置</param>
    /// <returns>更新后的规则，不存在则返回 null</returns>
    public async Task<AlertRuleResponse?> UpdateAlertRuleAsync(int id, AlertRuleRequest req)
    {
        var rule = await _db.Queryable<MonitorAlertRule>().FirstAsync(r => r.Id == id);
        if (rule is null) return null;

        rule.Name = req.Name;
        rule.Metric = req.Metric;
        rule.Operator = req.Operator;
        rule.Threshold = req.Threshold;
        rule.DurationSec = req.DurationSec ?? 300;
        await _db.Updateable(rule).ExecuteCommandAsync();
        return MapAlertRule(rule);
    }

    /// <summary>删除告警规则</summary>
    /// <param name="id">规则 ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteAlertRuleAsync(int id)
    {
        var rows = await _db.Deleteable<MonitorAlertRule>(id).ExecuteCommandAsync();
        return rows > 0;
    }

    // ======== 告警事件 ========

    /// <summary>获取告警事件历史（按触发时间倒序）</summary>
    /// <param name="limit">返回条数上限，默认 50</param>
    /// <returns>告警事件列表（含关联规则名称）</returns>
    public async Task<List<AlertEventResponse>> GetAlertEventsAsync(int limit = 50)
    {
        var events = await _db.Queryable<MonitorAlertEvent>()
            .LeftJoin<MonitorAlertRule>((e, r) => e.RuleId == r.Id)
            .OrderByDescending((e, r) => e.TriggeredAt)
            .Take(limit)
            .Select((e, r) => new AlertEventResponse(
                Id: e.Id,
                RuleName: r.Name,
                TriggeredAt: e.TriggeredAt,
                ResolvedAt: e.ResolvedAt,
                Message: e.Message,
                Severity: e.Severity
            ))
            .ToListAsync();
        return events;
    }

    /// <summary>写入告警事件（由 Worker 采集器调用）</summary>
    /// <param name="evt">告警事件实体</param>
    public async Task InsertAlertEventAsync(MonitorAlertEvent evt)
    {
        await _db.Insertable(evt).ExecuteCommandAsync();
    }

    // ======== 私有映射方法 ========

    private static ServerMetricsResponse MapMetric(MonitorServerMetric m) => new(
        CpuPct: m.CpuPct, MemPct: m.MemPct, MemUsed: m.MemUsed,
        MemTotal: m.MemTotal, DiskPct: m.DiskPct, NetIn: m.NetIn,
        NetOut: m.NetOut, Load1m: m.Load1m, Load5m: m.Load5m,
        Load15m: m.Load15m, Ts: m.Ts
    );

    private static SiteResponse MapSite(MonitorSite s) => new(
        Id: s.Id, Name: s.Name, Url: s.Url,
        IntervalSec: s.IntervalSec, TimeoutMs: s.TimeoutMs,
        IsActive: s.IsActive, CreatedAt: s.CreatedAt
    );

    private static AlertRuleResponse MapAlertRule(MonitorAlertRule r) => new(
        Id: r.Id, Name: r.Name, Metric: r.Metric,
        Operator: r.Operator, Threshold: r.Threshold,
        DurationSec: r.DurationSec, Enabled: r.Enabled,
        CreatedAt: r.CreatedAt
    );
}
