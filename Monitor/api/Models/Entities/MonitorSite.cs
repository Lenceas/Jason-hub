using SqlSugar;

namespace MonitorApi.Models.Entities;

/// <summary>站点配置表</summary>
[SugarTable("sites")]
public class MonitorSite
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "站点ID")]
    public int Id { get; set; }

    [SugarColumn(Length = 100, ColumnDescription = "站点名称")]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 500, ColumnDescription = "站点URL")]
    public string Url { get; set; } = string.Empty;

    [SugarColumn(DefaultValue = "60", ColumnDescription = "探测间隔（秒）")]
    public int IntervalSec { get; set; } = 60;

    [SugarColumn(DefaultValue = "5000", ColumnDescription = "超时时间（毫秒）")]
    public int TimeoutMs { get; set; } = 5000;

    [SugarColumn(Length = 10, DefaultValue = "GET", ColumnDescription = "HTTP方法")]
    public string Method { get; set; } = "GET";

    [SugarColumn(DefaultValue = "1", ColumnDescription = "是否启用")]
    public bool IsActive { get; set; } = true;

    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
