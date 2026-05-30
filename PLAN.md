# Jason-hub — 总体规划

> 本文档描述 Jason-hub 项目的全景规划、设计哲学、子项目生态关系及发展路线图。

---

## 一、项目愿景

**Jason-hub** 是一个个人项目聚合站（Monorepo），作者 **Lenceas（Jason / 卢杰晟）**。核心理念是：

> 一个入口，所有作品。

所有个人项目统一在 `lujiesheng.cn` 域名下，通过 Portfolio 主站聚合展示，各子项目独立部署、独立演进，形成完整的技术作品集。

---

## 二、项目全景

```
                        用户访问
                           │
                     ┌─────┴──────┐
                     │  HTTPS 443  │
                     └─────┬──────┘
                           │
                     ┌─────┴──────┐
                     │   Nginx     │ ← SSL termination + 反向代理
                     │  (主机)      │
                     └──┬───┬───┬──┬──────────────┘
                        │   │   │  │
              ┌─────────┘   │   │  └──────────────┐
              ▼             ▼   ▼                  ▼
       ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌─────────────────┐
       │ Portfolio │ │ Monitor  │ │ 子项目N   │ │ 基础设施服务      │
       │ :8000     │ │ :8001    │ │ :8002+    │ │ ┌─────────────┐ │
       │ Astro 6   │ │ Vue 3    │ │ Vue 3     │ │ │ Auth 服务    │ │
       └──────────┘ └────┬─────┘ └──────────┘ │ │ :8100        │ │
                         │                    │ │ Notification│ │
                         ▼                    │ │ :8110        │ │
                  ┌──────────┐                │ │ 任务调度 :8120│ │
                  │ Monitor  │ ← .NET 10 API  │ │ 消息队列 :8130│ │
                  │ :8051    │                │ └─────────────┘ │
                  └──────────┘                └────────┬────────┘
                         │                            │
              ┌──────────┴──────────┐                 │
              │                     │                 │
              ▼                     ▼                 │
       ┌──────────┐         ┌──────────┐             │
       │  MySQL   │         │  Redis   │ ← 全局基础设施
       │  8.4     │         │  8       │             │
       └──────────┘         └──────────┘             │
       ┌──────────┐                                  │
       │ MongoDB  │ ← 全局基础设施                    │
       │  8       │                                 │
       └──────────┘                                 │
              └──────────────────────────────────────┘
```

### 层级说明

| 层级 | 说明 |
|------|------|
| **接入层** | Nginx 80/443 → SSL termination → 按域名反代到容器 |
| **展示层** | Portfolio（Astro 6 静态站）作为统一入口 |
| **应用层** | 各子项目独立容器部署（Vue 3 前端 + .NET 10 API） |
| **基础设施服务层** | Auth 服务 :8100 / Notification 通知服务 :8110 / 任务调度 :8120 / 消息队列 :8130 |
| **基础设施层** | MySQL 8.4 + Redis 8 + MongoDB 8 全局共享 |

---

## 三、子项目生态

### 命名规范

| 类型 | 格式 | 示例 |
|------|------|------|
| 子项目目录 | PascalCase | `Monitor/` |
| 前端域名 | `<name>.lujiesheng.cn` | `monitor.lujiesheng.cn` |
| API 域名 | `api-<name>.lujiesheng.cn` | `api-monitor.lujiesheng.cn` |
| 前端端口 | 8000–8049 | 8001 |
| 子项目 API 端口 | 8050–8099 | 8051 |
| 基础设施服务端口 | 8100–8149 | 8100（Auth 服务） |

### 子项目与主站关系

- Portfolio 是**第 0 子项目**，也是唯一入口
- 所有子项目通过 Portfolio 的 `projects.json` 卡片跳转访问
- 子项目的代码、部署、版本号完全独立，不耦合 Portfolio
- 新增子项目 = 新建目录 → 开发 → DNS → Nginx → 加卡片

### 版本管理

| 范围 | 版本策略 |
|------|---------|
| 主仓库（main） | 语义化版本，合并子项目时 +1 minor |
| 子项目分支 | 独立版本号，从 v0.1.0 开始自由发版 |
| Portfolio | 在 package.json 中独立记录，不受子项目版本影响 |

详见 [STYLE_GUIDE.md](./STYLE_GUIDE.md#子项目版本管理)。

---

## 四、技术栈全景

| 模块 | 技术 | 版本 |
|------|------|------|
| 主站框架 | Astro | ^6.3.7 |
| 子项目前端 | Vue 3 + TypeScript | — |
| 子项目后端 | .NET | 10 |
| 基础设施服务 | .NET 10 + Minimal API | Auth 鉴权 / 通知 / 任务调度 / 消息队列 |
| ORM | SqlSugar | — |
| API 文档 | Scalar | — |
| 样式 | 原生 CSS + UnoCSS | — |
| 图表 | ECharts | — |
| 状态管理 | Pinia | — |
| 采集代理 | .NET 10 Background Worker | 内置在子项目 API 容器中 |
| 数据库 | MySQL / Redis / MongoDB | 8.4 / 8 / 8 |
| 容器 | Docker + Docker Compose | — |
| 服务器 | 腾讯云 2C4G / 70GB SSD / 6Mbps | Ubuntu 24.04 |
| 反向代理 | Nginx | — |
| SSL | acme.sh + Let's Encrypt | DNS-01 |
| CI/CD | GitHub Actions | SCP → docker compose |
| 版本控制 | Git + GitHub Flow | — |

---

## 五、域名规划

| 域名 | 指向 | 证书 |
|------|------|------|
| `lujiesheng.cn` | Portfolio :8000 | 一张 |
| `www.lujiesheng.cn` | Portfolio :8000（301 跳主域名） | 同上一张 |
| `monitor.lujiesheng.cn` | Monitor 前端 :8001 | 单独一张 |
| `api-monitor.lujiesheng.cn` | Monitor API :8051 | 单独一张 |
| `api-auth.lujiesheng.cn` | Auth 鉴权服务 :8100（含登录页） | 单独一张 |
| `api-notification.lujiesheng.cn` | Notification 通知服务 :8110 | 单独一张 |
| `<name>.lujiesheng.cn` | 未来子项目前端 | 各一张 |
| `api-<name>.lujiesheng.cn` | 未来子项目 API | 各一张 |

---

## 六、路线图

| 阶段 | 项目 | 时间 | 状态 |
|------|------|------|------|
| Phase 0 | Portfolio 主站 | 2026-05 | ✅ active |
| Phase 1 | Monitor 监控面板 | 2026-06 | 🚧 planning |
| Phase 2 | 个人博客 | 待定 | 📋 backlog |
| Phase 3 | IoT Dashboard | 待定 | 📋 backlog |
| Phase 4 | 更多子项目 | 待定 | 🔮 待定 |

### 选择子项目的原则

1. **技术多样性** — 每个子项目使用不同的技术组合，展示全面的技术能力
2. **实用价值** — 解决真实需求（监控、写作、数据可视化）
3. **渐进复杂度** — 从简单到复杂，逐步提升项目深度

---

## 七、设计原则

### 架构原则

- **松耦合**：子项目之间不直接依赖，仅通过基础设施层共享数据
- **独立演进**：每个子项目可独立开发、部署、迭代
- **统一入口**：所有子项目通过 Portfolio 发现和跳转
- **容器化**：所有服务以 Docker 容器运行，环境一致性

### 开发原则

- **约定优于配置**：目录结构、端口分配、域名命名有统一规则
- **文档驱动**：子项目动工前必须先出方案文档（PLAN.md）
- **自动化**：CI/CD 自动构建部署，减少手动操作
- **版本清晰**：语义化版本，每个发布都有明确的变更记录

### 样式原则

- 蓝白主色调贯穿所有子项目，保持品牌一致性
- Portfolio CSS 变量作为全局主题的源头
- 子项目可继承主站变量，也可按需扩展

---

## 八、基础设施服务方案

> 基础设施服务是全局共享的服务层，独立于子项目，提供鉴权、通知、任务调度、消息队列等公共能力。
> 端口范围 **8100–8149**，仅 Docker 内网互联（部分需对外提供 API 的除外）。

### 1. Auth 鉴权服务

Jason-hub 的统一鉴权中心，为所有子项目 API 和基础设施服务提供 JWT 签发与验证。

| 属性 | 说明 |
|------|------|
| 角色 | 基础设施服务（鉴权中心） |
| 端口 | 8100 |
| 域名 | `api-auth.lujiesheng.cn` |
| 技术栈 | .NET 10 + Minimal API + SqlSugar |
| 验证方式 | RS256 非对称签名，各服务本地验证 |
| 适用场景 | 用户登录、服务间调用 |

#### 健康检查设计

Auth 服务提供两个健康端点，应对不同场景：

| 端点 | 鉴权 | 使用者 | 用途 |
|------|------|--------|------|
| `GET /healthz` | ❌ 公开 | Nginx、Docker healthcheck、Monitor 站点监控 | 存活检测 + SSL 证书检查 |
| `GET /api/v1/auth/health` | ✅ Bearer JWT | Monitor 应用健康检查 | 深度检测（含数据库连接状态） |

> `GET /healthz` 是纯存活探针，仅限内网访问。Nginx 层面拦截公网对 `/healthz` 的请求（`return 404`），Docker healthcheck 直连容器内部端口获取。公网站点监控仅做 TLS 握手检测，不依赖 `/healthz`。

#### 为什么选 JWT + 本地验证

| 方案 | 优点 | 缺点 |
|------|------|------|
| 中心验证（每次调 Auth） | 可即时吊销 | 额外网络开销；Auth 宕机全瘫痪 |
| **JWT 本地验证 ← 选它** | 零网络开销、Auth 宕机不影响已签发令牌 | 不能即时吊销（等待过期） |

对于个人项目，即时吊销不是刚需，本地验证的稳定性和性能更关键。

#### 登录页与跨子域认证

登录页由 Auth API 容器直接提供（无需独立前端项目），通过 `HttpOnly Cookie` 实现跨子域共享。

##### 域名方案

| 功能 | 域名 | SSL |
|------|------|-----|
| 登录页面 | `GET https://api-auth.lujiesheng.cn/login` → HTML | ✅ 同域名已有证书 |
| 登录 API | `POST https://api-auth.lujiesheng.cn/api/v1/auth/login` | ✅ 同上 |

不需要单独的 `auth.lujiesheng.cn`，登录页由 Auth API 自身渲染一个简单 HTML 表单即可，零额外域名、零额外证书。

##### 认证流程

```
1. 用户访问 monitor.lujiesheng.cn
   → SPA 发现无有效 token
   → 302 跳转到 api-auth.lujiesheng.cn/login

2. 用户输入密码
   → Auth 验证成功
   → 写 HttpOnly Cookie（Domain=.lujiesheng.cn）
   → 302 跳回 monitor.lujiesheng.cn?token=<jwt>

3. SPA 拿到 token → 存 localStorage
   → 后续 API 走 Authorization: Bearer <jwt>
   → 切换到 blog.lujiesheng.cn 也自动带 cookie 登录
```

##### Cookie 配置

```http
Set-Cookie: jwt=<token>; Domain=.lujiesheng.cn; Path=/;
                        Secure; HttpOnly; SameSite=Lax
```

| 属性 | 作用 |
|------|------|
| `Domain=.lujiesheng.cn` | 所有 `*.lujiesheng.cn` 子域名生效 |
| `HttpOnly` | 防 XSS 窃取 token |
| `Secure` | 仅 HTTPS 传输 |
| `SameSite=Lax` | 允许同站 GET 跳转携带 |

##### API 验证优先级

后端统一验证逻辑支持双通道，方便 SPA 和服务间调用：

```
1. 优先读 Authorization: Bearer <token>
2. 无则读 Cookie: jwt=<token>
3. 皆无 → 401
```

#### API 设计

```
GET  /login                       ← 返回登录页 HTML（公开）
GET  /healthz                     ← 公开，无鉴权 —— Nginx / Docker healthcheck 用
GET  /api/v1/auth/health          ← 需 JWT —— Monitor 内部深度健康检查（含数据库连接）
POST /api/v1/auth/login           ← 用户名密码 → JWT（用户场景）
POST /api/v1/auth/token           ← ClientId/Secret → JWT（服务场景）
POST /api/v1/auth/refresh         ← 刷新令牌
GET  /api/v1/auth/public-key      ← 返回公钥（各服务启动时拉取并缓存）
```

#### JWT 载荷结构

```json
{
  "sub": "monitor-api",
  "type": "service",
  "role": "admin",
  "scopes": ["notification:write", "monitor:read"],
  "iat": 1746000000,
  "exp": 1746086400
}
```

| 字段 | 说明 |
|------|------|
| `type` | `user`（用户登录）或 `service`（服务间调用） |
| `scopes` | 权限范围，各 API 端点以此控制访问 |
| `sub` | 用户 ID 或 ClientId |
| `exp` | 过期时间（用户 token 24h，服务 token 1h） |

#### 数据库表

```sql
-- 用户表（管理员登录，位于 jason_auth 库）
CREATE TABLE auth_users (
    id              INT PRIMARY KEY AUTO_INCREMENT  COMMENT '用户ID',
    username        VARCHAR(100) NOT NULL UNIQUE    COMMENT '用户名',
    password_hash   VARCHAR(255) NOT NULL           COMMENT '密码哈希（BCrypt）',
    role            VARCHAR(50) DEFAULT 'admin'      COMMENT '角色：admin',
    failed_attempts INT DEFAULT 0                   COMMENT '连续登录失败次数',
    locked_until    DATETIME                        COMMENT '账户锁定截止时间',
    created_at      DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间'
) COMMENT='用户表 — 管理员登录';

-- 服务间调用凭证
CREATE TABLE auth_clients (
    id                INT PRIMARY KEY AUTO_INCREMENT    COMMENT '凭证ID',
    client_id         VARCHAR(100) NOT NULL UNIQUE      COMMENT '客户端ID',
    client_secret_hash VARCHAR(255) NOT NULL            COMMENT '客户端密钥哈希',
    name              VARCHAR(200) NOT NULL             COMMENT '服务名称',
    scopes            VARCHAR(500) NOT NULL             COMMENT '权限范围（逗号分隔）',
    is_active         TINYINT(1) DEFAULT 1              COMMENT '是否启用',
    created_at        DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间'
) COMMENT='服务间调用凭证表';

-- 刷新令牌
CREATE TABLE auth_refresh_tokens (
    id         BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '令牌ID',
    user_id    INT                                COMMENT '用户ID（关联auth_users）',
    token_hash VARCHAR(255) NOT NULL              COMMENT '令牌哈希',
    expires_at DATETIME NOT NULL                  COMMENT '过期时间',
    revoked    TINYINT(1) DEFAULT 0               COMMENT '是否已吊销',
    FOREIGN KEY (user_id) REFERENCES auth_users(id)
) COMMENT='刷新令牌表';
```

#### 爆破防御

Auth 服务是基础设施层的入口，虽然只有管理员使用，仍需防止暴力破解。做三层防御，**不引入验证码**：

| 层 | 措施 | 配置 | 实现 |
|----|------|------|------|
| **IP 限流** | 同 IP 每分钟最多 5 次登录 | `POST /api/v1/auth/login` | .NET 内置 `RateLimiter` 中间件（Token Bucket） |
| **账户锁定** | 连续 10 次密码错误 → 锁定 15 分钟 | `auth_users.locked_until` | 登录失败计数器，达标写锁定时间戳 |
| **日志告警** | 连续失败写入结构化日志 | Serilog → 文件 → Monitor Agent 采集 | 异常登录可追溯 |

##### IP 限流示例

```csharp
// Program.cs
app.UseRateLimiter(new RateLimiterOptions
{
    Limiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 5,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 5,
                AutoReplenish = true
            }))
});

// 仅限登录接口
app.MapPost("/api/v1/auth/login", ...).RequireRateLimiting();
```

##### 账户锁定逻辑（伪代码）

```
POST /api/v1/auth/login
  → 查用户
  → 检查 locked_until > now → 拒绝，返回 "账户已锁定，请 15 分钟后再试"
  → 验证密码
  → 成功 → 清 failed_attempts = 0 → 返回 JWT
  → 失败 → failed_attempts += 1
         → 如果 failed_attempts >= 10 → locked_until = now + 15min → 返回 "账户已锁定"
         → 否则返回 "密码错误"
```

个人项目场景下，这三层足以挡住绝大多数爆破尝试，且用户侧完全无感（不需要验证码、无需额外前端交互）。

#### 共享中间件

所有 API 项目引用同一个共享库 `Auth/Shared/`：

| 组件 | 职责 |
|------|------|
| `JwtValidator` | RS256 签名验证，缓存公钥（启动时 + 定期刷新） |
| `RequireScopeAttribute` | `[RequireScope("notification:write")]` 声明式权限控制 |
| `ServiceAuthHandler` | 解析 JWT 填充 `HttpContext`，提供 `CurrentUser` / `CurrentService` |

用法示例：

```csharp
// Monitor API 中
app.MapGet("/api/v1/alerts", () => { ... })
   .RequireScope("alerts:read");

// Notification API 中
app.MapPost("/api/v1/notify", () => { ... })
   .RequireScope("notification:write");
```

#### 部署检查清单

- [ ] 创建 `Auth/api/` 项目骨架（.NET 10 + Minimal API）
- [ ] 创建 `Auth/Shared/` 共享中间件库（作为 NuGet 本地包或项目引用）
- [ ] 编写 Dockerfile
- [ ] `docker-compose.yml` 追加 auth 服务（`:8100`）
- [ ] DNS 添加 `api-auth.lujiesheng.cn` → `81.71.136.3`
- [ ] SSL：acme.sh 申请 `api-auth.lujiesheng.cn` 证书
- [ ] Nginx 添加子域名 server 块
- [ ] GitHub Actions `deploy.yml` 增加 auth 构建命令
- [ ] 各子项目 API 接入共享中间件，配置公钥地址

---

## 九、数据库设计原则

> 所有子项目共用一个 MySQL 8.4 实例，按 Database 逻辑隔离。不分库分表、不做读写分离、不上多租户。

### 命名规范

所有数据库统一使用 `jason_<项目名>` 格式：

| 数据库 | 用途 |
|--------|------|
| `jason_auth` | Auth 鉴权（全局用户中心） |
| `jason_monitor` | Monitor 监控面板 |
| `jason_blog` | 未来：个人博客 |
| `jason_forum` | 未来：论坛 |

### 设计理念

| 问题 | 为什么不做 | 什么时候需要 |
|------|-----------|-------------|
| 读写分离 | 一台服务器，读 QPS 很低 | 读 QPS ≥ 5000 |
| 多租户 | 没有"租户"概念，只有不同类型的用户 | 需要 SaaS 化隔离数据 |
| 独立数据库实例 | 一个 MySQL 容器跑所有库，零额外运维 | 单个库达到服务器资源瓶颈 |

### 库结构

```
MySQL 8.4 容器 (:3306)
├── jason_auth              ← Auth 鉴权（全局用户中心）
│   ├── auth_users          ← 所有用户（你 + 未来注册用户）
│   ├── auth_clients        ← 服务间调用凭证
│   └── auth_refresh_tokens ← 刷新令牌
│
├── jason_monitor           ← Monitor 监控面板
│   ├── server_metrics
│   ├── sites / uptime_records
│   ├── container_snapshots
│   ├── health_records
│   ├── alert_rules / alert_events
│   └── ci_deployments
│
├── jason_blog              ← 未来：个人博客
│   ├── posts / categories / tags
│   └── comments
│
├── jason_forum             ← 未来：论坛
│   ├── threads / posts
│   └── categories
│
└── 更多 jason_<项目名>...
```

### 用户中心全局共享

`jason_auth` 数据库是所有子项目共用的全局用户中心，其他子项目通过 `user_id` 外键引用：

```sql
-- 博客文章引用 jason_auth 用户
CREATE TABLE jason_blog.posts (
    id        INT PRIMARY KEY AUTO_INCREMENT,
    title     VARCHAR(200) NOT NULL,
    author_id INT NOT NULL,           -- REFERENCES jason_auth.auth_users(id)
    -- ...
);

-- 论坛帖子引用 jason_auth 用户
CREATE TABLE jason_forum.threads (
    id      INT PRIMARY KEY AUTO_INCREMENT,
    title   VARCHAR(200) NOT NULL,
    user_id INT NOT NULL,            -- REFERENCES jason_auth.auth_users(id)
    -- ...
);
```

> 每个子项目不存用户数据，用户统一存在 `jason_auth` 库中。一个账号登录所有子项目。

### 连接配置

各服务通过环境变量配置各自数据库的连接串：

```env
AUTH_DB_CONNECTION=Server=127.0.0.1;Port=3306;Database=jason_auth;User=root;Password=<pw>;
MONITOR_DB_CONNECTION=Server=127.0.0.1;Port=3306;Database=jason_monitor;User=root;Password=<pw>;
BLOG_DB_CONNECTION=Server=127.0.0.1;Port=3306;Database=jason_blog;User=root;Password=<pw>;
```

SqlSugar 多数据库注册：

```csharp
// 每个服务只注册自己的数据库
services.AddSqlSugar(cfg => {
    cfg.ConnectionString = Config["AuthDb"];   // 或 MonitorDb / BlogDb
    cfg.DbType = DbType.MySql;
});
```

### 成长路径

```
数据量            数据库方案                    架构变化
────────────────────────────────────────────────────────
< 100 万行        单 MySQL 实例，按 Database 隔离    无变化
100 万 - 1000 万  单实例 + 索引优化 + 缓存           零架构变化
1000 万 - 1 亿    读写分离（MySQL 主从）             加只读节点
> 1 亿            分库（按子项目拆独立实例）          改连接配置
```

当前阶段用最简方案，未来扩展只需改连接串，无需调整代码。

---

## 十、关键决策记录

| 日期 | 决策 | 参考 |
|------|------|------|
| 2026-05-30 | 端口段重定义：三段式（前端/子项目API/基础设施服务 8000-8149） | `AGENTS.md` |
| 2026-05-30 | Auth 鉴权服务纳入基础设施层：JWT + RS256 本地验证 | `PLAN.md` |
| 2026-05-30 | 通知服务从 Monitor 抽取为独立基础设施服务（Notification :8110）| `PLAN.md` |
| 2026-05-30 | 子项目文档三件套：PLAN.md / CHANGELOG.md / README.md | `STYLE_GUIDE.md` |
| 2026-05-30 | 双轨发布流：子项目分支独立发版 + main 主发布流 | `RELEASE.md` |
| 2026-05-30 | 子项目独立版本号，main 合并时 +1 minor | `STYLE_GUIDE.md` |
| 2026-05-30 | 子项目统一 Vue 3 + .NET 10 | `ARCHITECTURE.md` |
| 2026-05-30 | 数据库设计原则：单实例按 Database 隔离，不分库分表不读写分离不多租户 | `PLAN.md` |
| 2026-05-30 | Monitor 方案确定（6 大模块 + 7 张表 + Agent 采集） | `Monitor/PLAN.md` |
| 2026-05-29 | 基础设施层 MySQL 8.4 + Redis 8 + MongoDB 8 | `docker-compose.yml` |
| 2026-05-27 | Monorepo 五种命名规范 | `STYLE_GUIDE.md` |

---

> **本文档随项目发展持续更新，重大架构变更需同步修改。**
