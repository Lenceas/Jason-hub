using SqlSugar;

namespace AuthApi.Models.Entities;

/// <summary>
/// 用户账户表（auth_users）
/// <para>存储系统用户信息，包括密码哈希、角色、登录安全策略相关字段。</para>
/// </summary>
[SugarTable("auth_users")]
public class AuthUser
{
    /// <summary>用户 ID（自增主键）</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
    public int Id { get; set; }

    /// <summary>用户名（唯一标识，用于登录）</summary>
    [SugarColumn(ColumnName = "username")]
    public string Username { get; set; } = "";

    /// <summary>密码哈希（BCrypt 加密存储，不保存明文）</summary>
    [SugarColumn(ColumnName = "password_hash")]
    public string PasswordHash { get; set; } = "";

    /// <summary>
    /// 用户角色
    /// <para>admin — 管理员，拥有全部权限</para>
    /// <para>其他值可按需扩展（如 user / operator）</para>
    /// </summary>
    [SugarColumn(ColumnName = "role")]
    public string Role { get; set; } = "admin";

    /// <summary>昵称（显示名，为空时回退到 username）</summary>
    [SugarColumn(ColumnName = "nickname")]
    public string? Nickname { get; set; }

    /// <summary>邮箱地址</summary>
    [SugarColumn(ColumnName = "email")]
    public string? Email { get; set; }

    /// <summary>手机号</summary>
    [SugarColumn(ColumnName = "phone")]
    public string? Phone { get; set; }

    /// <summary>头像 URL</summary>
    [SugarColumn(ColumnName = "avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>个人简介</summary>
    [SugarColumn(ColumnName = "bio")]
    public string? Bio { get; set; }

    /// <summary>账户状态（enabled / disabled）</summary>
    [SugarColumn(ColumnName = "status")]
    public string Status { get; set; } = "enabled";

    /// <summary>最后登录时间</summary>
    [SugarColumn(ColumnName = "last_login_at", ColumnDataType = "DATETIME(3)")]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>最后登录 IP</summary>
    [SugarColumn(ColumnName = "last_login_ip")]
    public string? LastLoginIp { get; set; }

    /// <summary>最后登录城市</summary>
    [SugarColumn(ColumnName = "last_login_city")]
    public string? LastLoginCity { get; set; }

    /// <summary>连续登录失败次数（达到阈值后触发锁定）</summary>
    [SugarColumn(ColumnName = "failed_attempts")]
    public int FailedAttempts { get; set; }

    /// <summary>锁定截止时间（null 表示未锁定；未到期时禁止登录）</summary>
    [SugarColumn(ColumnName = "locked_until", ColumnDataType = "DATETIME(3)")]
    public DateTime? LockedUntil { get; set; }

    /// <summary>账户创建时间</summary>
    [SugarColumn(ColumnName = "created_at", ColumnDataType = "DATETIME(3)")]
    public DateTime CreatedAt { get; set; }

    /// <summary>记录更新时间</summary>
    [SugarColumn(ColumnName = "updated_at", ColumnDataType = "DATETIME(3)")]
    public DateTime? UpdatedAt { get; set; }
}
