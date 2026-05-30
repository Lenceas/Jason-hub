# Auth 鉴权服务 — 方案设计

> 状态：`dev` · 基础设施服务
> 本文档记录 Auth 鉴权服务的设计与实现。

---

## 一、项目定位

Jason-hub 统一鉴权中心，为所有子项目 API 和基础设施服务提供 JWT 签发与验证。

| 属性 | 说明 |
|------|------|
| 角色 | 基础设施服务（鉴权中心） |
| 端口 | 8100（容器内 8080） |
| 域名 | `api-auth.lujiesheng.cn` |
| 技术栈 | .NET 10 + Minimal API + SqlSugar + BCrypt |
| 验证方式 | RS256 非对称签名，各服务本地验证 |
| 适用场景 | 用户登录、服务间调用 |

## 二、项目结构

```
Auth/
├── api/                    ← .NET 10 Web API
│   ├── Program.cs          ← 入口 + 所有端点
│   ├── Services/
│   │   ├── JwtService.cs   ← RSA 密钥管理 + JWT 签发
│   │   └── AuthService.cs  ← 登录校验 + 爆破防御
│   ├── Models/
│   │   └── AuthModels.cs   ← 请求/响应 DTO
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Dockerfile
├── Shared/                 ← 共享中间件库（各子项目 API 引用）
│   ├── JwtValidator.cs     ← RS256 签名验证
│   ├── AuthHandler.cs      ← 解析 JWT 中间件 + RequireScope 过滤器
│   └── AuthShared.csproj
├── PLAN.md
├── CHANGELOG.md
└── README.md
```

## 三、API 端点

| 方法 | 路径 | 鉴权 | 说明 |
|------|------|------|------|
| GET | `/login` | ❌ 公开 | 登录页 HTML |
| GET | `/healthz` | ❌ 公开 | 存活检测（供 Nginx / Docker healthcheck） |
| POST | `/api/v1/auth/login` | ❌ 公开（限流 5次/分钟/IP） | 用户名密码 → JWT + HttpOnly Cookie |
| POST | `/api/v1/auth/token` | ❌ 公开 | ClientId/Secret → 服务 JWT |
| POST | `/api/v1/auth/refresh` | ❌ 公开 | 刷新令牌 → 新 JWT |
| GET | `/api/v1/auth/public-key` | ❌ 公开 | RSA 公钥 PEM |
| GET | `/api/v1/auth/health` | ✅ Bearer JWT | 深度健康检查（含数据库连接） |

## 四、数据库

数据库 `jason_auth`，三张表：

| 表名 | 用途 |
|------|------|
| `auth_users` | 管理员登录（username / password_hash / role / failed_attempts / locked_until） |
| `auth_clients` | 服务间调用凭证（client_id / client_secret_hash / scopes） |
| `auth_refresh_tokens` | 刷新令牌（token_hash / expires_at / revoked） |

## 五、认证流程

```
用户访问 monitor.lujiesheng.cn
  → SPA 发现无 token
  → 302 跳转到 api-auth.lujiesheng.cn/login

用户输入密码
  → Auth 验证成功
  → 写 HttpOnly Cookie（Domain=.lujiesheng.cn）
  → 302 跳回来源页 + token 参数

SPA 拿到 token → localStorage
后续 API 走 Authorization: Bearer <jwt>
```

## 六、爆破防御

| 层 | 措施 | 配置 |
|----|------|------|
| IP 限流 | 同 IP 每分钟最多 5 次登录 | RateLimiter + FixedWindow |
| 账户锁定 | 连续 10 次密码错误 → 锁定 15 分钟 | `auth_users.failed_attempts` + `locked_until` |
| 日志告警 | 失败写入结构化日志 | ILogger |

## 七、共享中间件（Auth/Shared/）

各子项目 API 通过 NuGet 引用 `AuthShared` 库：

```csharp
// 1. 启动时拉取并缓存公钥
var validator = new JwtValidator(publicKeyPem);

// 2. 注册中间件
builder.Services.AddSingleton<AuthHandler>();

// 3. 端点加权限
app.MapGet("/api/v1/alerts", () => { ... })
   .RequireScope("alerts:read");
```

---

> **本文档随开发持续更新。**
