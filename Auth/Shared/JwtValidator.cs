using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthShared;

/// <summary>
/// RS256 JWT 验证器 — 各子项目 API 引用此库本地验证 token
/// </summary>
public class JwtValidator
{
    private readonly TokenValidationParameters _validationParameters;
    private readonly RsaSecurityKey _publicKey;
    private readonly JsonWebTokenHandler _handler = new();

    public RsaSecurityKey PublicKey => _publicKey;

    public JwtValidator(string publicKeyPem)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        _publicKey = new RsaSecurityKey(rsa);

        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "jason-auth",
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = _publicKey
        };
    }

    /// <summary>验证 JWT 并返回 ClaimsPrincipal</summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var result = _handler.ValidateTokenAsync(token, _validationParameters)
                .GetAwaiter().GetResult();
            if (!result.IsValid) return null;
            var identity = result.ClaimsIdentity ?? new ClaimsIdentity();
            if (string.IsNullOrEmpty(identity.AuthenticationType))
                identity = new ClaimsIdentity(identity.Claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>检查是否拥有指定权限</summary>
    public static bool HasScope(ClaimsPrincipal user, string requiredScope)
    {
        var scopes = user.FindFirst("scopes")?.Value;
        if (string.IsNullOrEmpty(scopes)) return false;
        return scopes.Split(',').Contains(requiredScope);
    }

    /// <summary>获取当前用户 ID</summary>
    public static string? GetUserId(ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

    /// <summary>获取令牌类型 (user/service)</summary>
    public static string? GetTokenType(ClaimsPrincipal user)
        => user.FindFirst("type")?.Value;
}
