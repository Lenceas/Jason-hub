using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Services;

/// <summary>
/// JWT 签发服务 — RS256 非对称签名
/// </summary>
public class JwtService : IDisposable
{
    private readonly RSA _rsa;
    private readonly RsaSecurityKey _privateKey;
    private readonly RsaSecurityKey _publicKey;
    private readonly JsonWebTokenHandler _handler = new();
    private readonly IConfiguration _config;
    private readonly object _signLock = new();

    public RsaSecurityKey PublicKey => _publicKey;
    public string PublicKeyPem { get; }

    public JwtService(IConfiguration config)
    {
        _config = config;
        _rsa = RSA.Create(2048);

        var keysPath = _config.GetValue<string>("Jwt:KeysPath") ?? "/app/keys";
        var privateKeyFile = Path.Combine(keysPath, "private_key.pem");
        var publicKeyFile = Path.Combine(keysPath, "public_key.pem");

        if (File.Exists(privateKeyFile) && File.Exists(publicKeyFile))
        {
            // 加载已有密钥
            _rsa.ImportFromPem(File.ReadAllText(privateKeyFile));
            PublicKeyPem = File.ReadAllText(publicKeyFile);
        }
        else
        {
            // 生成新密钥对
            Directory.CreateDirectory(keysPath);
            var privateKey = _rsa.ExportPkcs8PrivateKey();
            var publicKey = _rsa.ExportSubjectPublicKeyInfo();
            PublicKeyPem = new string(PemEncoding.Write("PUBLIC KEY", publicKey));

            File.WriteAllText(privateKeyFile, new string(PemEncoding.Write("PRIVATE KEY", privateKey)));
            File.WriteAllText(publicKeyFile, PublicKeyPem);

            // 限制文件权限（仅 Linux）
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(privateKeyFile, UnixFileMode.UserRead);
        }

        _privateKey = new RsaSecurityKey(_rsa) { KeyId = "jason-auth-rs256" };
        _publicKey = new RsaSecurityKey(_rsa);
    }

    /// <summary>签发用户 token（24h 过期）</summary>
    public string CreateUserToken(int userId, string username, string role)
    {
        var expiration = TimeSpan.FromHours(24);
        var scopes = role switch
        {
            "admin" => "admin:all,auth:read,monitor:read,notification:write",
            _ => "auth:read"
        };

        return CreateToken(new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["username"] = username,
            ["type"] = "user",
            ["role"] = role,
            ["scopes"] = scopes
        }, expiration);
    }

    /// <summary>签发服务 token（1h 过期）</summary>
    public string CreateServiceToken(string clientId, string name, string scopes)
    {
        return CreateToken(new Dictionary<string, object>
        {
            ["sub"] = clientId,
            ["name"] = name,
            ["type"] = "service",
            ["role"] = "service",
            ["scopes"] = scopes
        }, TimeSpan.FromHours(1));
    }

    /// <summary>创建刷新令牌（30d 过期）</summary>
    public string CreateRefreshToken(int userId)
    {
        return CreateToken(new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["type"] = "refresh",
            ["purpose"] = "refresh"
        }, TimeSpan.FromDays(30));
    }

    /// <summary>RSA-OAEP 解密（前端加密密码用）</summary>
    public string Decrypt(byte[] ciphertext)
    {
        var plain = _rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(plain);
    }

    private string CreateToken(Dictionary<string, object> claims, TimeSpan expiration)
    {
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "jason-auth",
            Claims = claims,
            NotBefore = now,
            Expires = now.Add(expiration),
            IssuedAt = now,
            SigningCredentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256)
        };

        lock (_signLock)
        {
            return _handler.CreateToken(descriptor);
        }
    }

    public void Dispose() => _rsa?.Dispose();
}
