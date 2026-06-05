# Auth 鉴权服务 — 方案设计

> 状态：`active` · 基础设施服务 · 已部署上线
> 本文档记录 Auth 鉴权服务的设计与实现。

---

## 一、项目定位

Jason-hub 统一鉴权中心，为所有子项目 API 和基础设施服务提供 JWT 签发与验证。

| 属性 | 说明 |
|------|------|
| 角色 | 基础设施服务（鉴权中心） |
| 端口 | 8100 |
| 域名 | `api-auth.lujiesheng.cn` |
| 技术栈 | .NET 10 + Minimal API + SqlSugar + BCrypt |
| 验证方式 | RS256 非对称签名，各服务本地验证 |
| 适用场景 | 用户登录、服务间调用 |

## 二、项目结构

```
Auth/
├── api/
│   ├── Program.cs                   ← 入口（服务注册 + 中间件）
│   ├── Endpoints/
│   │   └── AuthEndpoints.cs         ← 全部 API 端点（链式调用 + OpenAPI 描述）
│   ├── Models/
│   │   ├── AuthModels.cs            ← 请求/响应 DTO
│   │   └── Entities/                ← CodeFirst 实体
│   │       ├── AuthUser.cs
│   │       ├── AuthRefreshToken.cs
│   │       └── AuthClient.cs
│   ├── Pages/
│   │   └── LoginPage.cs             ← 登录页 HTML
│   ├── Services/
│   │   ├── AuthService.cs           ← 认证业务逻辑
│   │   ├── JwtService.cs            ← RSA 密钥管理 + JWT 签发
│   │   └── Ip2RegionService.cs      ← IP 城市解析（ip2region 离线库）
│   └── Dockerfile
├── Shared/
│   ├── JwtValidator.cs              ← RS256 签名验证
│   ├── AuthHandler.cs               ← JWT 解析中间件 + RequireScope
│   └── AuthShared.csproj
├── PLAN.md
├── CHANGELOG.md
└── README.md
```

## 三、API 端点

| 方法 | 路径 | 鉴权 | 说明 |
|------|------|------|------|
| GET | `/login` | ❌ 公开 | 统一鉴权登录页（支持 `?redirect=`） |
| GET | `/logout` | ❌ 公开 | 退出登录，清除 Cookie（支持 `?redirect=`） |
| GET | `/healthz` | ❌ 公开 | 存活检测 |
| GET | `/api/v1/auth/health` | ✅ JWT | 深度健康检查（含数据库连通性） |
| POST | `/api/v1/auth/login` | ❌ 公开（限流 5次/分钟/IP） | 用户名密码 → JWT + HttpOnly Cookie |
| POST | `/api/v1/auth/token` | ❌ 公开 | ClientId/Secret → 服务 JWT |
| POST | `/api/v1/auth/refresh` | ❌ 公开 | RefreshToken → 新 JWT |
| GET | `/api/v1/auth/me` | ✅ JWT | 获取当前用户信息（含 nickname/email/city 等） |
| GET | `/api/v1/auth/public-key` | ❌ 公开 | RSA 公钥 PEM |
| GET | `/api/v1/docs` | ✅ JWT | 重定向到 Scalar API 文档 |

## 四、数据库

数据库 `jason_auth`，三张表（CodeFirst 自动维护）：

| 表名 | 字段 | 用途 |
|------|------|------|
| `auth_users` | id / username / password_hash / nickname / email / phone / avatar_url / bio / role / status / failed_attempts / locked_until / last_login_at / last_login_ip / last_login_city / created_at / updated_at | 用户账户（含登录审计信息） |
| `auth_clients` | id / client_id / client_secret_hash / name / scopes / is_active / created_at | 服务间调用凭证 |
| `auth_refresh_tokens` | id / user_id / token_hash / expires_at / revoked | 刷新令牌 |

## 五、认证流程

```
用户访问 lujiesheng.cn
  → Portfolio 首页加载
  → JS fetch /api/v1/auth/me（带 Cookie）
  ├── 200 → 右上角显示用户信息（昵称/角色/城市）
  └── 401 → 显示「登录」按钮

用户点击「登录」
  → 跳转 api-auth.lujiesheng.cn/login?redirect=https://lujiesheng.cn
  → 输入用户名密码
  → Auth 验证成功 + IP 城市解析
  → 写 HttpOnly Cookie（Domain=.lujiesheng.cn）
  → 跳回 lujiesheng.cn

Portfolio 各子项目通过 Cookie 自动携带 JWT
后续 API 走 Authorization: Bearer <jwt> 或 Cookie

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
