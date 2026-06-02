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

    public AuthService(ISqlSugarClient db, JwtService jwt, JwtValidator validator, ILogger<AuthService> logger)
    {
        _db = db;
        _jwt = jwt;
        _validator = validator;
        _logger = logger;
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

        await _db.Insertable(new AuthRefreshToken
        {
            UserId = user.Id,
            TokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        }).ExecuteCommandAsync();

        _logger.LogInformation("[审计] 登录成功 [IP: {IP}] [UA: {UA}] [User: {User}] [Role: {Role}]",
            remoteIp, userAgent, user.Username, user.Role);

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

