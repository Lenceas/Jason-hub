using AuthApi.Models;
using AuthShared;
using SqlSugar;

namespace AuthApi.Services;

/// <summary>
/// Auth 业务服务 — 登录校验、令牌管理、爆破防御
/// </summary>
public class AuthService
{
    private readonly ISqlSugarClient _db;
    private readonly JwtService _jwt;
    private readonly JwtValidator _validator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ISqlSugarClient db, JwtService jwt, JwtValidator validator, ILogger<AuthService> logger)
    {
        _db = db;
        _jwt = jwt;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>用户密码登录</summary>
    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var user = await _db.Queryable<AuthUser>()
            .FirstAsync(u => u.Username == request.Username);

        if (user == null)
        {
            _logger.LogWarning("登录失败：用户不存在 [{User}]", request.Username);
            return null;
        }

        // 检查是否锁定
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            _logger.LogWarning("登录失败：账户已锁定 [{User}]", request.Username);
            return null;
        }

        // 验证密码
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // 原子递增失败计数，避免并发绕过（fix bug 2）
            await _db.Updateable<AuthUser>()
                .SetColumns(u => u.FailedAttempts == u.FailedAttempts + 1)
                .Where(u => u.Id == user.Id)
                .ExecuteCommandAsync();

            // 重新查一次，判断是否需要锁定
            var updated = await _db.Queryable<AuthUser>().FirstAsync(u => u.Id == user.Id);
            if (updated!.FailedAttempts >= 10)
            {
                await _db.Updateable<AuthUser>()
                    .SetColumns(u => u.LockedUntil == DateTime.UtcNow.AddMinutes(15))
                    .Where(u => u.Id == user.Id)
                    .ExecuteCommandAsync();
                _logger.LogWarning("账户已锁定 [{User}]，连续失败 {Count} 次", request.Username, updated.FailedAttempts);
            }
            return null;
        }

        // 登录成功，重置失败计数
        await _db.Updateable<AuthUser>()
            .SetColumns(u => new AuthUser
            {
                FailedAttempts = 0,
                LockedUntil = null
            })
            .Where(u => u.Id == user.Id)
            .ExecuteCommandAsync();

        var token = _jwt.CreateUserToken(user.Id, user.Username, user.Role);
        var refreshToken = _jwt.CreateRefreshToken(user.Id);

        // 持久化刷新令牌
        await _db.Insertable(new AuthRefreshToken
        {
            UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }).ExecuteCommandAsync();

        _logger.LogInformation("登录成功 [{User}] role={Role}", user.Username, user.Role);

        return new LoginResponse(token, 86400, user.Username, user.Role, refreshToken);
    }

    /// <summary>服务间调用 — Client Credentials 认证</summary>
    public async Task<JwtTokenResponse?> AuthenticateService(TokenRequest request)
    {
        var client = await _db.Queryable<AuthClient>()
            .FirstAsync(c => c.ClientId == request.ClientId && c.IsActive);

        if (client == null || !BCrypt.Net.BCrypt.Verify(request.ClientSecret, client.ClientSecretHash))
        {
            _logger.LogWarning("服务认证失败：{ClientId}", request.ClientId);
            return null;
        }

        var token = _jwt.CreateServiceToken(client.ClientId, client.Name, client.Scopes);
        return new JwtTokenResponse(token, "Bearer", 3600);
    }

    /// <summary>刷新 token</summary>
    public async Task<LoginResponse?> RefreshToken(string refreshToken)
    {
        // 查数据库验证 refresh token
        var allTokens = await _db.Queryable<AuthRefreshToken>()
            .Where(t => !t.Revoked && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        AuthRefreshToken? matched = null;
        foreach (var t in allTokens)
        {
            if (BCrypt.Net.BCrypt.Verify(refreshToken, t.TokenHash))
            {
                matched = t;
                break;
            }
        }

        if (matched == null)
        {
            _logger.LogWarning("刷新令牌无效或已过期");
            return null;
        }

        var user = await _db.Queryable<AuthUser>().FirstAsync(u => u.Id == matched.UserId);
        if (user == null) return null;

        // 吊销旧令牌
        await _db.Updateable<AuthRefreshToken>()
            .SetColumns(t => t.Revoked == true)
            .Where(t => t.Id == matched.Id)
            .ExecuteCommandAsync();

        // 签发新令牌
        var token = _jwt.CreateUserToken(user.Id, user.Username, user.Role);
        var newRefresh = _jwt.CreateRefreshToken(user.Id);

        await _db.Insertable(new AuthRefreshToken
        {
            UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(newRefresh),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }).ExecuteCommandAsync();

        return new LoginResponse(token, 86400, user.Username, user.Role, newRefresh);
    }
}

// SQL 映射实体（注意：数据库列名为 snake_case）
[SugarTable("auth_users")]
public class AuthUser
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
    public int Id { get; set; }
    [SugarColumn(ColumnName = "username")]
    public string Username { get; set; } = "";
    [SugarColumn(ColumnName = "password_hash")]
    public string PasswordHash { get; set; } = "";
    [SugarColumn(ColumnName = "role")]
    public string Role { get; set; } = "admin";
    [SugarColumn(ColumnName = "failed_attempts")]
    public int FailedAttempts { get; set; }
    [SugarColumn(ColumnName = "locked_until")]
    public DateTime? LockedUntil { get; set; }
    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; }
}

[SugarTable("auth_refresh_tokens")]
public class AuthRefreshToken
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
    public long Id { get; set; }
    [SugarColumn(ColumnName = "user_id")]
    public int UserId { get; set; }
    [SugarColumn(ColumnName = "token_hash")]
    public string TokenHash { get; set; } = "";
    [SugarColumn(ColumnName = "expires_at")]
    public DateTime ExpiresAt { get; set; }
    [SugarColumn(ColumnName = "revoked")]
    public bool Revoked { get; set; }
}

[SugarTable("auth_clients")]
public class AuthClient
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
    public int Id { get; set; }
    [SugarColumn(ColumnName = "client_id")]
    public string ClientId { get; set; } = "";
    [SugarColumn(ColumnName = "client_secret_hash")]
    public string ClientSecretHash { get; set; } = "";
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = "";
    [SugarColumn(ColumnName = "scopes")]
    public string Scopes { get; set; } = "";
    [SugarColumn(ColumnName = "is_active")]
    public bool IsActive { get; set; }
    [SugarColumn(ColumnName = "created_at")]
    public DateTime CreatedAt { get; set; }
}
