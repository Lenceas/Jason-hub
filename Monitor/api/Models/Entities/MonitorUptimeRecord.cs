using SqlSugar;

namespace MonitorApi.Models.Entities;

/// <summary>站点检查记录表</summary>
[SugarTable("uptime_records")]
[SugarIndex("idx_site_checked", nameof(SiteId), OrderByType.Asc, nameof(CheckedAt), OrderByType.Desc)]
public class MonitorUptimeRecord
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
    public long Id { get; set; }

    [SugarColumn(ColumnDescription = "站点ID（关联sites）")]
    public int SiteId { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "HTTP状态码")]
    public int? StatusCode { get; set; }

    [SugarColumn(IsNullable = true, ColumnDescription = "响应时间（毫秒）")]
    public int? ResponseMs { get; set; }

    [SugarColumn(ColumnDescription = "检查时间")]
    public DateTime CheckedAt { get; set; }

    [SugarColumn(ColumnDescription = "是否正常")]
    public bool IsOk { get; set; }
}
