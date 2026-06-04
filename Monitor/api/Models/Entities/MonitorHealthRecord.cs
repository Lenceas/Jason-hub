using SqlSugar;

namespace MonitorApi.Models.Entities;

/// <summary>健康检查记录表</summary>
[SugarTable("health_records")]
public class MonitorHealthRecord
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "检查时间")]
    public DateTime Ts { get; set; }

    [SugarColumn(Length = 100, ColumnDescription = "服务名称")]
    public string Service { get; set; } = string.Empty;

    [SugarColumn(Length = 500, ColumnDescription = "检查端点")]
    public string? Endpoint { get; set; }

    [SugarColumn(Length = 20, ColumnDescription = "状态码/结果")]
    public string? Status { get; set; }

    [SugarColumn(ColumnDescription = "延迟（毫秒）")]
    public int? LatencyMs { get; set; }
}
