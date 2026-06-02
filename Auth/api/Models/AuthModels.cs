using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AuthApi.Models;

/// <summary>用户登录请求</summary>
public record LoginRequest(
    [property: Description("用户名（至少 5 位）")]
    [property: JsonPropertyName("username")]
    string Username,

    [property: Description("密码（明文或 RSA-OAEP 加密后 Base64，至少 8 位）")]
    [property: JsonPropertyName("password")]
    string Password,

    [property: Description("密码是否经过 RSA-OAEP 前端加密，true 时后端自动解密")]
    [property: JsonPropertyName("encrypted")]
    bool Encrypted = false
);

/// <summary>服务间调用认证请求</summary>
public record TokenRequest(
    [property: Description("客户端 ID（唯一标识，由 auth_clients 表注册）")]
    [property: JsonPropertyName("client_id")]
    string ClientId,

    [property: Description("客户端密钥（明文，服务端 BCrypt 校验）")]
    [property: JsonPropertyName("client_secret")]
    string ClientSecret
);

/// <summary>刷新 AccessToken 请求</summary>
public record RefreshRequest(
    [property: Description("刷新令牌（30 天有效，一次性使用）")]
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken
);

/// <summary>JWT Token 响应（服务间调用认证用）</summary>
public record JwtTokenResponse(
    [property: Description("JWT AccessToken")]
    [property: JsonPropertyName("access_token")]
    string AccessToken,

    [property: Description("令牌类型（固定 Bearer）")]
    [property: JsonPropertyName("token_type")]
    string TokenType,

    [property: Description("有效期（秒）")]
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,

    [property: Description("刷新令牌（可选）")]
    [property: JsonPropertyName("refresh_token")]
    string? RefreshToken = null
);

/// <summary>用户登录响应</summary>
public record LoginResponse(
    [property: Description("JWT AccessToken（24 小时有效）")]
    [property: JsonPropertyName("access_token")]
    string AccessToken,

    [property: Description("有效期（秒）")]
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,

    [property: Description("用户名")]
    [property: JsonPropertyName("username")]
    string Username,

    [property: Description("用户角色（admin / user）")]
    [property: JsonPropertyName("role")]
    string Role,

    [property: Description("刷新令牌（30 天有效，一次性使用）")]
    [property: JsonPropertyName("refresh_token")]
    string? RefreshToken = null,

    [property: Description("剩余尝试次数（登录失败时返回）")]
    [property: JsonPropertyName("remaining_attempts")]
    int RemainingAttempts = 0,

    [property: Description("锁定剩余秒数（账户锁定时返回）")]
    [property: JsonPropertyName("locked_remaining_seconds")]
    int LockedRemainingSeconds = 0
);
