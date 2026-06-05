using SqlSugar;

namespace MonitorApi.Models.Entities;

/// <summary>服务器指标表</summary>
[SugarTable("server_metrics")]
[SugarIndex("idx_ts", nameof(Ts), OrderByType.Asc)]
public class MonitorServerMetric
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "采集时间戳")]
    public DateTime Ts { get; set; }

    [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnDescription = "CPU使用率（%）")]
    public decimal? CpuPct { get; set; }

    [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnDescription = "内存使用率（%）")]
    public decimal? MemPct { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "已用内存（字节）")]
    public long? MemUsed { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "总内存（字节）")]
    public long? MemTotal { get; set; }

    [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnDescription = "磁盘使用率（%）")]
    public decimal? DiskPct { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "已用磁盘（字节）")]
    public long? DiskUsed { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "总磁盘（字节）")]
    public long? DiskTotal { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "入网流量（字节）")]
    public long? NetIn { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "出网流量（字节）")]
    public long? NetOut { get; set; }

    [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnDescription = "1分钟负载")]
    public decimal? Load1m { get; set; }

    [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnDescription = "5分钟负载")]
    public decimal? Load5m { get; set; }

    [SugarColumn(DecimalDigits = 2, IsNullable = true, ColumnDescription = "15分钟负载")]
    public decimal? Load15m { get; set; }
}
