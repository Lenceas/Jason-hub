using SqlSugar;

namespace AuthApi.Models.Entities;

/// <summary>
/// 服务客户端表（auth_clients）
/// <para>存储注册的服务间调用客户端信息。每个客户端有唯一的 client_id 和加密的 client_secret。</para>
/// <para>用于 OAuth2 Client Credentials 模式，签发服务 Token（1 小时有效期）。</para>
/// </summary>
[SugarTable("auth_clients")]
[SugarIndex("idx_client_id", nameof(ClientId), OrderByType.Asc)]
public class AuthClient
{
    /// <summary>记录 ID（自增主键）</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
    public int Id { get; set; }

    /// <summary>客户端 ID（唯一标识，用于认证）</summary>
    [SugarColumn(ColumnName = "client_id")]
    public string ClientId { get; set; } = "";

    /// <summary>客户端密钥哈希（BCrypt，不保存原文）</summary>
    [SugarColumn(ColumnName = "client_secret_hash")]
    public string ClientSecretHash { get; set; } = "";

    /// <summary>客户端名称（用于日志和展示）</summary>
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = "";

    /// <summary>授权作用域（逗号分隔，如 "monitor:read,notification:write"）</summary>
    [SugarColumn(ColumnName = "scopes")]
    public string Scopes { get; set; } = "";

    /// <summary>是否激活（false 表示停用，禁止签发 Token）</summary>
    [SugarColumn(ColumnName = "is_active")]
    public bool IsActive { get; set; }

    /// <summary>客户端创建时间</summary>
    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; }
}
