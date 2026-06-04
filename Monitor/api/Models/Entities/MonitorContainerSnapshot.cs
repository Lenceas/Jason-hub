using SqlSugar;

namespace MonitorApi.Models.Entities;

/// <summary>容器快照表</summary>
[SugarTable("container_snapshots")]
public class MonitorContainerSnapshot
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "采集时间戳")]
    public DateTime Ts { get; set; }

    [SugarColumn(Length = 200, ColumnDescription = "容器名称")]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 50, ColumnDescription = "容器状态")]
    public string? Status { get; set; }

    [SugarColumn(DecimalDigits = 2, ColumnDescription = "CPU使用率")]
    public decimal? CpuPct { get; set; }

    [SugarColumn(ColumnDescription = "内存使用量（字节）")]
    public long? MemUsage { get; set; }

    [SugarColumn(ColumnDescription = "内存限制（字节）")]
    public long? MemLimit { get; set; }
}
