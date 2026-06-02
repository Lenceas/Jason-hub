using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AuthApi.Models;

/// <summary>用户登录请求</summary>
public record LoginRequest(
    /// <summary>用户名（至少 5 位）</summary>
    [property: Description("用户名（至少 5 位）")]
    [property: JsonPropertyName("username")]
    string Username,

    /// <summary>密码（明文或 RSA-OAEP 加密后 Base64，至少 8 位）</summary>
    [property: Description("密码（明文或 RSA-OAEP 加密后 Base64，至少 8 位）")]
    [property: JsonPropertyName("password")]
    string Password,

    /// <summary>密码是否经过 RSA-OAEP 前端加密（true 时后端自动解密）</summary>
    [property: Description("密码是否经过 RSA-OAEP 前端加密，true 时后端自动解密")]
    [property: JsonPropertyName("encrypted")]
    bool Encrypted = false
);

/// <summary>服务间调用认证请求</summary>
public record TokenRequest(
    /// <summary>客户端 ID（唯一标识，由 auth_clients 表注册）</summary>
    [property: Description("客户端 ID（唯一标识，由 auth_clients 表注册）")]
    [property: JsonPropertyName("client_id")]
    string ClientId,

    /// <summary>客户端密钥（明文，服务端 BCrypt 校验）</summary>
    [property: Description("客户端密钥（明文，服务端 BCrypt 校验）")]
    [property: JsonPropertyName("client_secret")]
    string ClientSecret
);

/// <summary>刷新 AccessToken 请求</summary>
public record RefreshRequest(
    /// <summary>刷新令牌（30 天有效，一次性使用）</summary>
    [property: Description("刷新令牌（30 天有效，一次性使用）")]
    [property: JsonPropertyName("refresh_token")]
    string RefreshToken
);

/// <summary>JWT Token 响应（服务间调用认证用）</summary>
public record JwtTokenResponse(
    /// <summary>JWT AccessToken（Bearer Token）</summary>
    [property: Description("JWT AccessToken")]
    [property: JsonPropertyName("access_token")]
    string AccessToken,

    /// <summary>令牌类型（固定 Bearer）</summary>
    [property: Description("令牌类型（固定 Bearer）")]
    [property: JsonPropertyName("token_type")]
    string TokenType,

    /// <summary>有效期（秒）</summary>
    [property: Description("有效期（秒）")]
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,

    /// <summary>刷新令牌（可选，仅用户登录返回）</summary>
    [property: Description("刷新令牌（可选）")]
    [property: JsonPropertyName("refresh_token")]
    string? RefreshToken = null
);

/// <summary>用户登录响应</summary>
public record LoginResponse(
    /// <summary>JWT AccessToken（24 小时有效）</summary>
    [property: Description("JWT AccessToken（24 小时有效）")]
    [property: JsonPropertyName("access_token")]
    string AccessToken,

    /// <summary>有效期（秒，固定 86400）</summary>
    [property: Description("有效期（秒）")]
    [property: JsonPropertyName("expires_in")]
    int ExpiresIn,

    /// <summary>用户名</summary>
    [property: Description("用户名")]
    [property: JsonPropertyName("username")]
    string Username,

    /// <summary>用户角色（admin / user）</summary>
    [property: Description("用户角色（admin / user）")]
    [property: JsonPropertyName("role")]
    string Role,

    /// <summary>刷新令牌（30 天有效，一次性使用）</summary>
    [property: Description("刷新令牌（30 天有效，一次性使用）")]
    [property: JsonPropertyName("refresh_token")]
    string? RefreshToken = null,

    /// <summary>剩余尝试次数（登录失败时返回）</summary>
    [property: Description("剩余尝试次数（登录失败时返回）")]
    [property: JsonPropertyName("remaining_attempts")]
    int RemainingAttempts = 0,

    /// <summary>锁定剩余秒数（账户锁定时返回）</summary>
    [property: Description("锁定剩余秒数（账户锁定时返回）")]
    [property: JsonPropertyName("locked_remaining_seconds")]
    int LockedRemainingSeconds = 0
);
