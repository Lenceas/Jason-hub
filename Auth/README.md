# Auth 鉴权服务

> [中文](#zh) · [English](#en)

---

<h2 id="zh">中文</h2>

Jason-hub 统一鉴权中心 — JWT + RS256 非对称签名。

### 在线地址

| 类型 | 地址 |
|------|------|
| 登录页 | `api-auth.lujiesheng.cn/login` |
| API 文档 | `api-auth.lujiesheng.cn/scalar/v1` |

### 技术栈

| 模块 | 技术 |
|------|------|
| 框架 | .NET 10 + Minimal API |
| ORM | SqlSugar + CodeFirst |
| 认证 | JWT RS256（非对称） |
| 密码 | BCrypt |
| IP 解析 | ip2region（国产离线库） |
| 数据库 | MySQL 8.4（`jason_auth`） |
| API 文档 | Scalar · BluePlanet 主题 |

### 项目结构

```
Auth/
├── api/
│   ├── Program.cs                   ← 入口（服务注册 + 中间件）
│   ├── AuthApi.csproj               ← .NET 10 项目文件
│   ├── Endpoints/
│   │   └── AuthEndpoints.cs         ← 全部 API 端点（链式调用风格）
│   ├── Models/
│   │   ├── AuthModels.cs            ← 请求/响应 DTO
│   │   └── Entities/
│   │       ├── AuthUser.cs          ← 用户实体
│   │       ├── AuthRefreshToken.cs  ← 刷新令牌实体
│   │       └── AuthClient.cs        ← 服务客户端实体
│   ├── Pages/
│   │   └── LoginPage.cs             ← 登录页 HTML
│   └── Services/
│       ├── AuthService.cs           ← 认证业务逻辑
│       ├── JwtService.cs            ← JWT 签发
│       └── Ip2RegionService.cs      ← IP 城市解析
└── Shared/
    ├── AuthHandler.cs               ← JWT 解析中间件
    └── JwtValidator.cs              ← RS256 验证器
```

### 开发

```bash
# 启动（需本地 MySQL 隧道）
cd Auth/api
ASPNETCORE_ENVIRONMENT=Development dotnet run

# 本地访问
http://localhost:8100/healthz     # 健康检查
http://localhost:8100/login        # 登录页
http://localhost:8100/scalar/v1    # API 文档
```

### API 端点

| 路径 | 方法 | 说明 | 认证 |
|------|------|------|------|
| `GET /login` | GET | 统一鉴权登录页 | 公开 |
| `GET /logout` | GET | 退出登录，清除 Cookie | 公开 |
| `GET /healthz` | GET | 公开健康检查 | 公开 |
| `GET /api/v1/auth/health` | GET | 深度健康检查（含数据库） | JWT |
| `POST /api/v1/auth/login` | POST | 用户密码登录 | 公开 |
| `POST /api/v1/auth/token` | POST | 服务间调用认证 | ClientId/Secret |
| `POST /api/v1/auth/refresh` | POST | 刷新 AccessToken | RefreshToken |
| `GET /api/v1/auth/me` | GET | 获取当前登录用户信息 | JWT |
| `GET /api/v1/auth/public-key` | GET | RSA 公钥 | 公开 |
| `GET /api/v1/docs` | GET | 跳转到 Scalar 文档 | JWT |

### 共享库

`Auth/Shared/` 提供给各子项目 API 引用，实现本地 JWT 验证：

```bash
dotnet add reference ../Shared/AuthShared.csproj
```

```csharp
app.MapGet("/api/v1/alerts", () => { ... })
   .RequireScope("alerts:read");
```

### 部署

```bash
docker compose up -d auth
```

端口：8100
域名：`api-auth.lujiesheng.cn`
数据库：`jason_auth`（MySQL 8.4）

---

<h2 id="en">English</h2>

Jason-hub unified authentication center — JWT with RS256 asymmetric signing.

### Live URLs

| Type | URL |
|------|-----|
| Login Page | `api-auth.lujiesheng.cn/login` |
| API Docs | `api-auth.lujiesheng.cn/scalar/v1` |

### Tech Stack

| Module | Technology |
|--------|-----------|
| Framework | .NET 10 + Minimal API |
| ORM | SqlSugar + CodeFirst |
| Auth | JWT RS256 (asymmetric) |
| Password | BCrypt |
| IP Geolocation | ip2region (offline, Chinese) |
| Database | MySQL 8.4 (`jason_auth`) |
| API Docs | Scalar · BluePlanet theme |

### Project Structure

```
Auth/
├── api/
│   ├── Program.cs                   ← Entry point
│   ├── AuthApi.csproj
│   ├── Endpoints/
│   │   └── AuthEndpoints.cs         ← All API endpoints
│   ├── Models/
│   │   ├── AuthModels.cs            ← Request/Response DTOs
│   │   └── Entities/
│   │       ├── AuthUser.cs
│   │       ├── AuthRefreshToken.cs
│   │       └── AuthClient.cs
│   ├── Pages/
│   │   └── LoginPage.cs             ← Login page HTML
│   └── Services/
│       ├── AuthService.cs           ← Business logic
│       ├── JwtService.cs            ← JWT issuance
│       └── Ip2RegionService.cs      ← IP geolocation
└── Shared/
    ├── AuthHandler.cs               ← JWT middleware
    └── JwtValidator.cs              ← RS256 validator
```

### Development

```bash
cd Auth/api
ASPNETCORE_ENVIRONMENT=Development dotnet run

# Local access
http://localhost:8100/healthz
http://localhost:8100/login
http://localhost:8100/scalar/v1
```

### API Endpoints

| Path | Method | Description | Auth |
|------|--------|-------------|------|
| `GET /login` | GET | Unified login page | Public |
| `GET /logout` | GET | Logout, clear cookie | Public |
| `GET /healthz` | GET | Public health check | Public |
| `GET /api/v1/auth/health` | GET | Deep health check (+DB) | JWT |
| `POST /api/v1/auth/login` | POST | Password login | Public |
| `POST /api/v1/auth/token` | POST | Service-to-service auth | ClientId/Secret |
| `POST /api/v1/auth/refresh` | POST | Refresh AccessToken | RefreshToken |
| `GET /api/v1/auth/me` | GET | Current user info | JWT |
| `GET /api/v1/auth/public-key` | GET | RSA public key | Public |
| `GET /api/v1/docs` | GET | Redirect to Scalar docs | JWT |

### Shared Library

`Auth/Shared/` provides JWT validation for other sub-projects:

```bash
dotnet add reference ../Shared/AuthShared.csproj
```

### Deployment

```bash
docker compose up -d auth
```

Port: 8100
Domain: `api-auth.lujiesheng.cn`
Database: `jason_auth` (MySQL 8.4)
