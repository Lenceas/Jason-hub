using SqlSugar;

namespace MonitorApi.Models.Entities;

/// <summary>告警规则表</summary>
[SugarTable("alert_rules")]
public class MonitorAlertRule
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "规则ID")]
    public int Id { get; set; }

    [SugarColumn(Length = 200, ColumnDescription = "规则名称")]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 100, ColumnDescription = "监控指标名")]
    public string Metric { get; set; } = string.Empty;

    [SugarColumn(Length = 10, ColumnDescription = "比较运算符（> / < / >= / <=）")]
    public string Operator { get; set; } = string.Empty;

    [SugarColumn(DecimalDigits = 2, ColumnDescription = "告警阈值")]
    public decimal Threshold { get; set; }

    [SugarColumn(DefaultValue = "300", ColumnDescription = "持续时长（秒）")]
    public int DurationSec { get; set; } = 300;

    [SugarColumn(DefaultValue = "1", ColumnDescription = "是否启用")]
    public bool Enabled { get; set; } = true;

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
