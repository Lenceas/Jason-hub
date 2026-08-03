using AuthApi.Models;
using AuthApi.Models.Entities;
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
    private readonly Ip2RegionService _ipGeo;

    public AuthService(ISqlSugarClient db, JwtService jwt, JwtValidator validator, ILogger<AuthService> logger, Ip2RegionService ipGeo)
    {
        _db = db;
        _jwt = jwt;
        _validator = validator;
        _logger = logger;
        _ipGeo = ipGeo;
    }

    /// <summary>
    /// 初始化超级管理员 — 用户不存在且配置了初始密码时创建（幂等）
    /// </summary>
    /// <param name="username">管理员用户名（来自 InitAdmin:Username，默认 admin）</param>
    /// <param name="password">初始明文密码（仅用于 BCrypt 哈希，来自环境变量，不落库不落码）</param>
    /// <returns>是否执行了创建</returns>
    public async Task<bool> EnsureInitAdminAsync(string? username, string? password)
    {
        username = username?.Trim();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogInformation("未配置初始管理员密码（InitAdmin:Password），跳过管理员初始化");
            return false;
        }

        var exists = await _db.Queryable<AuthUser>().AnyAsync(u => u.Username == username);
        if (exists)
        {
            _logger.LogInformation("初始管理员 {User} 已存在，跳过初始化", username);
            return false;
        }

        var now = DateTime.UtcNow;
        await _db.Insertable(new AuthUser
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "admin",
            Status = "enabled",
            Nickname = username,
            FailedAttempts = 0,
            CreatedAt = now,
            UpdatedAt = now
        }).ExecuteCommandAsync();

        _logger.LogInformation("[初始化] 超级管理员 {User} 创建成功（BCrypt 哈希存储，不保存明文）", username);
        return true;
    }

    /// <summary>用户密码登录 — 返回 null 表示成功以外的失败（用户不存在/锁定/密码错误）</summary>
    public async Task<LoginResponse?> Login(LoginRequest request, string remoteIp, string userAgent)
    {
        // 服务端输入校验
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Trim().Length < 5)
        {
            _logger.LogWarning("[审计] 登录请求用户名过短 [IP: {IP}] [UA: {UA}]", remoteIp, userAgent);
            return new LoginResponse("", 0, "", "", null, 0, 0);
        }
        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        {
            _logger.LogWarning("[审计] 登录请求密码过短 [IP: {IP}] [UA: {UA}] [User: {User}]", remoteIp, userAgent, request.Username);
            return new LoginResponse("", 0, "", "", null, 0, 0);
        }

        var user = await _db.Queryable<AuthUser>()
            .FirstAsync(u => u.Username == request.Username);

        if (user == null)
        {
            _logger.LogWarning("[审计] 登录失败 [IP: {IP}] [UA: {UA}] [User: {User}] 原因: 用户不存在", remoteIp, userAgent, request.Username);
            return null; // 统一返回，不暴露用户是否存在
        }

        // 检查是否锁定
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
        {
            var remainingSec = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalSeconds;
            _logger.LogWarning("[审计] 账户已锁定 [IP: {IP}] [UA: {UA}] [User: {User}] 剩余: {Sec}s", remoteIp, userAgent, request.Username, remainingSec);
            return new LoginResponse("", 0, "", "", null, 10 - user.FailedAttempts, remainingSec);
        }

        // 验证密码
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // 原子递增失败计数
            await _db.Updateable<AuthUser>()
                .SetColumns(u => u.FailedAttempts == u.FailedAttempts + 1)
                .Where(u => u.Id == user.Id)
                .ExecuteCommandAsync();

            var updated = await _db.Queryable<AuthUser>().FirstAsync(u => u.Id == user.Id);
            var remaining = 10 - updated!.FailedAttempts;
            var lockedSec = 0;

            if (updated.FailedAttempts >= 10)
            {
                await _db.Updateable<AuthUser>()
                    .SetColumns(u => u.LockedUntil == DateTime.UtcNow.AddMinutes(15))
                    .Where(u => u.Id == user.Id)
                    .ExecuteCommandAsync();
                lockedSec = 900;
                _logger.LogWarning("[审计] 账户已锁定 [IP: {IP}] [UA: {UA}] [User: {User}] 剩余尝试: 0", remoteIp, userAgent, request.Username);
            }
            else
            {
                _logger.LogWarning("[审计] 登录失败 [IP: {IP}] [UA: {UA}] [User: {User}] 密码错误 剩余尝试: {Rem}", remoteIp, userAgent, request.Username, remaining);
            }
            return new LoginResponse("", 0, "", "", null, remaining > 0 ? remaining : 0, lockedSec);
        }

        // 登录成功
        var city = await _ipGeo.GetCityAsync(remoteIp);

        await _db.Updateable<AuthUser>()
            .SetColumns(u => new AuthUser
            {
                FailedAttempts = 0,
                LockedUntil = null,
                LastLoginAt = DateTime.UtcNow,
                LastLoginIp = remoteIp,
                LastLoginCity = city,
                UpdatedAt = DateTime.UtcNow
            })
            .Where(u => u.Id == user.Id)
            .ExecuteCommandAsync();

        var token = _jwt.CreateUserToken(user.Id, user.Username, user.Role);
        var refreshToken = _jwt.CreateRefreshToken(user.Id);

        await _db.Insertable(new AuthRefreshToken
        {
            UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }).ExecuteCommandAsync();

        _logger.LogInformation("[审计] 登录成功 [IP: {IP}] [UA: {UA}] [User: {User}] [Role: {Role}] [City: {City}]",
            remoteIp, userAgent, user.Username, user.Role, city ?? "unknown");

        return new LoginResponse(token, 86400, user.Username, user.Role, refreshToken, 10, 0);
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
