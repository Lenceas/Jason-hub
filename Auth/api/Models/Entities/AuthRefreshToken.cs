using SqlSugar;

namespace AuthApi.Models.Entities;

/// <summary>
/// 刷新令牌表（auth_refresh_tokens）
/// <para>存储长期有效的刷新令牌（30天），用于在 AccessToken 过期后静默续期。</para>
/// <para>不保存令牌原文，仅保存 BCrypt 哈希值。支持吊销（revoked）标记。</para>
/// </summary>
[SugarTable("auth_refresh_tokens")]
[SugarIndex("idx_token_hash", nameof(TokenHash), OrderByType.Asc)]
[SugarIndex("idx_user_revoked_expires", nameof(UserId), OrderByType.Asc, nameof(Revoked), OrderByType.Asc, nameof(ExpiresAt), OrderByType.Asc)]
public class AuthRefreshToken
{
    /// <summary>记录 ID（自增主键）</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
    public long Id { get; set; }

    /// <summary>关联的用户 ID（对应 auth_users.id）</summary>
    [SugarColumn(ColumnName = "user_id")]
    public int UserId { get; set; }

    /// <summary>刷新令牌哈希（BCrypt，不保存原文）</summary>
    [SugarColumn(ColumnName = "token_hash")]
    public string TokenHash { get; set; } = "";

    /// <summary>过期时间（创建后 30 天）</summary>
    [SugarColumn(ColumnName = "expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>是否已吊销（true 表示已使用或主动失效）</summary>
    [SugarColumn(ColumnName = "revoked")]
    public bool Revoked { get; set; }
}
