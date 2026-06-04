using SqlSugar;

namespace MonitorApi.Models.Entities;

/// <summary>告警事件表</summary>
[SugarTable("alert_events")]
public class MonitorAlertEvent
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "事件ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "关联规则ID")]
    public int RuleId { get; set; }

    [SugarColumn(ColumnDescription = "触发时间")]
    public DateTime TriggeredAt { get; set; }

    [SugarColumn(ColumnDescription = "恢复时间")]
    public DateTime? ResolvedAt { get; set; }

    [SugarColumn(ColumnDataType = "TEXT", ColumnDescription = "告警消息")]
    public string? Message { get; set; }

    [SugarColumn(Length = 20, DefaultValue = "warning", ColumnDescription = "严重级别（warning/critical）")]
    public string Severity { get; set; } = "warning";
}
