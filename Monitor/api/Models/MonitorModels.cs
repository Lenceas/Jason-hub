using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MonitorApi.Models;

// ======== 服务器指标 ========

/// <summary>服务器实时指标响应</summary>
public record ServerMetricsResponse(
    [property: Description("CPU使用率（%）")]
    [property: JsonPropertyName("cpu_pct")]
    decimal? CpuPct,

    [property: Description("内存使用率（%）")]
    [property: JsonPropertyName("mem_pct")]
    decimal? MemPct,

    [property: Description("已用内存（字节）")]
    [property: JsonPropertyName("mem_used")]
    long? MemUsed,

    [property: Description("总内存（字节）")]
    [property: JsonPropertyName("mem_total")]
    long? MemTotal,

    [property: Description("磁盘使用率（%）")]
    [property: JsonPropertyName("disk_pct")]
    decimal? DiskPct,

    [property: Description("已用磁盘（字节）")]
    [property: JsonPropertyName("disk_used")]
    long? DiskUsed,

    [property: Description("总磁盘（字节）")]
    [property: JsonPropertyName("disk_total")]
    long? DiskTotal,

    [property: Description("入网流量（字节）")]
    [property: JsonPropertyName("net_in")]
    long? NetIn,

    [property: Description("出网流量（字节）")]
    [property: JsonPropertyName("net_out")]
    long? NetOut,

    [property: Description("1分钟负载")]
    [property: JsonPropertyName("load_1m")]
    decimal? Load1m,

    [property: Description("5分钟负载")]
    [property: JsonPropertyName("load_5m")]
    decimal? Load5m,

    [property: Description("15分钟负载")]
    [property: JsonPropertyName("load_15m")]
    decimal? Load15m,

    [property: Description("采集时间")]
    [property: JsonPropertyName("ts")]
    DateTime Ts
);

/// <summary>历史指标查询响应</summary>
public record ServerHistoryResponse(
    [property: Description("指标列表")]
    [property: JsonPropertyName("metrics")]
    List<ServerMetricsResponse> Metrics
);

// ======== 站点监控 ========

/// <summary>新增监控站点请求</summary>
public record SiteRequest(
    [property: Description("站点名称")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("站点URL")]
    [property: JsonPropertyName("url")]
    string Url,

    [property: Description("探测间隔（秒），默认60")]
    [property: JsonPropertyName("interval_sec")]
    int? IntervalSec,

    [property: Description("超时时间（毫秒），默认5000")]
    [property: JsonPropertyName("timeout_ms")]
    int? TimeoutMs
);

/// <summary>监控站点响应</summary>
public record SiteResponse(
    [property: Description("站点ID")]
    [property: JsonPropertyName("id")]
    int Id,

    [property: Description("站点名称")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("站点URL")]
    [property: JsonPropertyName("url")]
    string Url,

    [property: Description("探测间隔（秒）")]
    [property: JsonPropertyName("interval_sec")]
    int IntervalSec,

    [property: Description("超时时间（毫秒）")]
    [property: JsonPropertyName("timeout_ms")]
    int TimeoutMs,

    [property: Description("是否启用")]
    [property: JsonPropertyName("is_active")]
    bool IsActive,

    [property: Description("创建时间")]
    [property: JsonPropertyName("created_at")]
    DateTime CreatedAt
);

// ======== 可用性 ========

/// <summary>站点可用性检查记录</summary>
public record UptimeRecordResponse(
    [property: Description("记录ID")]
    [property: JsonPropertyName("id")]
    long Id,

    [property: Description("HTTP状态码")]
    [property: JsonPropertyName("status_code")]
    int? StatusCode,

    [property: Description("响应时间（毫秒）")]
    [property: JsonPropertyName("response_ms")]
    int? ResponseMs,

    [property: Description("是否正常")]
    [property: JsonPropertyName("is_ok")]
    bool IsOk,

    [property: Description("检查时间")]
    [property: JsonPropertyName("checked_at")]
    DateTime CheckedAt
);

/// <summary>站点可用性历史响应</summary>
public record UptimeHistoryResponse(
    [property: Description("站点ID")]
    [property: JsonPropertyName("site_id")]
    int SiteId,

    [property: Description("可用率（%）")]
    [property: JsonPropertyName("uptime_pct")]
    decimal UptimePct,

    [property: Description("24h内检查次数")]
    [property: JsonPropertyName("total_checks")]
    int TotalChecks,

    [property: Description("正常次数")]
    [property: JsonPropertyName("ok_checks")]
    int OkChecks,

    [property: Description("检查记录列表")]
    [property: JsonPropertyName("records")]
    List<UptimeRecordResponse> Records
);

// ======== 容器 ========

/// <summary>容器快照响应</summary>
public record ContainerSnapshotResponse(
    [property: Description("容器名称")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("状态")]
    [property: JsonPropertyName("status")]
    string? Status,

    [property: Description("CPU使用率（%）")]
    [property: JsonPropertyName("cpu_pct")]
    decimal? CpuPct,

    [property: Description("内存使用量（字节）")]
    [property: JsonPropertyName("mem_usage")]
    long? MemUsage,

    [property: Description("内存限制（字节）")]
    [property: JsonPropertyName("mem_limit")]
    long? MemLimit,

    [property: Description("采集时间")]
    [property: JsonPropertyName("ts")]
    DateTime Ts
);

// ======== 健康检查 ========

/// <summary>健康检查记录响应</summary>
public record HealthRecordResponse(
    [property: Description("服务名称")]
    [property: JsonPropertyName("service")]
    string Service,

    [property: Description("状态")]
    [property: JsonPropertyName("status")]
    string? Status,

    [property: Description("延迟（毫秒）")]
    [property: JsonPropertyName("latency_ms")]
    int? LatencyMs,

    [property: Description("检查时间")]
    [property: JsonPropertyName("ts")]
    DateTime Ts
);

// ======== 告警规则 ========

/// <summary>创建/修改告警规则请求</summary>
public record AlertRuleRequest(
    [property: Description("规则名称")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("监控指标")]
    [property: JsonPropertyName("metric")]
    string Metric,

    [property: Description("比较运算符（>/</>=/<=）")]
    [property: JsonPropertyName("operator")]
    string Operator,

    [property: Description("告警阈值")]
    [property: JsonPropertyName("threshold")]
    decimal Threshold,

    [property: Description("持续时长（秒），默认300")]
    [property: JsonPropertyName("duration_sec")]
    int? DurationSec
);

/// <summary>告警规则响应</summary>
public record AlertRuleResponse(
    [property: Description("规则ID")]
    [property: JsonPropertyName("id")]
    int Id,

    [property: Description("规则名称")]
    [property: JsonPropertyName("name")]
    string Name,

    [property: Description("监控指标")]
    [property: JsonPropertyName("metric")]
    string Metric,

    [property: Description("比较运算符")]
    [property: JsonPropertyName("operator")]
    string Operator,

    [property: Description("告警阈值")]
    [property: JsonPropertyName("threshold")]
    decimal Threshold,

    [property: Description("持续时长（秒）")]
    [property: JsonPropertyName("duration_sec")]
    int DurationSec,

    [property: Description("是否启用")]
    [property: JsonPropertyName("enabled")]
    bool Enabled,

    [property: Description("创建时间")]
    [property: JsonPropertyName("created_at")]
    DateTime CreatedAt
);

// ======== 告警事件 ========

/// <summary>告警事件响应</summary>
public record AlertEventResponse(
    [property: Description("事件ID")]
    [property: JsonPropertyName("id")]
    long Id,

    [property: Description("关联规则名称")]
    [property: JsonPropertyName("rule_name")]
    string? RuleName,

    [property: Description("触发时间")]
    [property: JsonPropertyName("triggered_at")]
    DateTime TriggeredAt,

    [property: Description("恢复时间")]
    [property: JsonPropertyName("resolved_at")]
    DateTime? ResolvedAt,

    [property: Description("告警消息")]
    [property: JsonPropertyName("message")]
    string? Message,

    [property: Description("严重级别")]
    [property: JsonPropertyName("severity")]
    string Severity
);
