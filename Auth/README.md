# Auth 鉴权服务

Jason-hub 统一鉴权中心 — JWT + RS256 非对称签名。

## 技术栈

| 模块 | 技术 |
|------|------|
| 框架 | .NET 10 + Minimal API |
| ORM | SqlSugar |
| 认证 | JWT RS256（非对称） |
| 密码 | BCrypt |
| 数据库 | MySQL 8.4（`jason_auth`） |

## 开发

```bash
# 启动（需本地 MySQL 隧道）
cd Auth/api
ASPNETCORE_ENVIRONMENT=Development dotnet run

# 本地访问
http://localhost:5100/healthz
http://localhost:5100/login
```

## API 端点

| 路径 | 说明 |
|------|------|
| `GET /healthz` | 公开健康检查 |
| `GET /login` | 登录页 |
| `POST /api/v1/auth/login` | 用户登录 → JWT |
| `POST /api/v1/auth/token` | 服务认证 → JWT |
| `POST /api/v1/auth/refresh` | 刷新令牌 |
| `GET /api/v1/auth/public-key` | RSA 公钥 |
| `GET /api/v1/auth/health` | 深度健康检查（需 JWT） |

## 共享库

`Auth/Shared/` 提供给各子项目 API 引用，实现本地 JWT 验证：

```bash
dotnet add reference ../Shared/AuthShared.csproj
```

```csharp
app.MapGet("/api/v1/alerts", () => { ... })
   .RequireScope("alerts:read");
```

## 部署

```bash
docker compose up -d auth
```

端口：8100（容器内 8080）
域名：`api-auth.lujiesheng.cn`
