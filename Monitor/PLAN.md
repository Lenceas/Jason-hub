# Monitor 监控面板 — 方案设计

> 状态：`planning` · 第一子项目
> 本文档为立项方案，开发前经用户确认，后续实施严格按此执行。

---

## 一、项目定位

Jason-hub 第一个子项目，统一监控平台。对服务器、Docker 容器、站点可用性、应用健康进行实时监控并支持多渠道告警通知。

| 项目 | 前端 | API | 端口 |
|------|------|-----|------|
| Monitor 前端 | `monitor.lujiesheng.cn` | — | 8001 |
| Monitor API | — | `api-monitor.lujiesheng.cn` | 8051 |

---

## 二、五大功能模块

> **MVP 范围**：以下五大模块全部实现，第一期即完整功能上线。

### 1. 🖥️ 服务器监控

| 指标 | 展示方式 | 数据来源 |
|------|---------|---------|
| CPU 使用率 | 实时仪表盘 + 24h 折线图 | .NET Worker Service 定时采集 |
| 内存使用率 | 仪表盘 + 趋势图 | 同上 |
| 磁盘分区 | 环图 + 列表 | 同上 |
| Docker 存储 | 单独展示 `/var/lib/docker` 分区用量 | 同上（含 overlay2 存储驱动占用） |
| 网络流量 | 上下行折线图 | 同上 |
| 系统负载/运行时间 | 指标卡片 | 同上 |

### 2. 🐳 Docker 容器监控

| 功能 | 说明 |
|------|------|
| 容器列表 | 实时状态、CPU/内存占用、端口映射 |
| 容器详情 | 日志查看、资源曲线 |
| 镜像列表 | 镜像大小、版本、数量 |
| 一键启停 | 快速 restart/stop 容器 |

### 3. 🌐 站点监控

| 功能 | 说明 |
|------|------|
| HTTP 可用性检查 | 定时探测各站点状态码 |
| 响应时间追踪 | 延迟曲线、P99/P95 统计 |
| SSL 证书到期 | 自动检查剩余天数，提前预警 |
| 可用率报表 | 日/周/月 SLA 百分比 |

##### 站点监控列表（规划）

| 站点 | 域名 | 检查方式 | 鉴权 |
|------|------|---------|------|
| Portfolio 主站 | `lujiesheng.cn` | HTTP GET `/` | ❌ 公开 |
| Monitor 前端 | `monitor.lujiesheng.cn` | HTTP GET `/` | ❌ 公开 |
| Monitor API | `api-monitor.lujiesheng.cn` | HTTP GET `/healthz` | ❌ 公开 |
| Auth 服务 | `api-auth.lujiesheng.cn` | HTTP GET `/login`（登录页） | ❌ 无 `/healthz` |
| Notification 通知 | `api-notification.lujiesheng.cn` | HTTP GET `/healthz` | ❌ 公开 |

> 站点监控检查的是"公网可达性"和"SSL 证书"，不使用 JWT 鉴权。Auth 服务的 `/healthz` 仅限内网（Nginx 公网拦截），站点监控改为 TLS 握手检测。其余 API 服务提供公开的 `/healthz` 端点。

#### 扩展：Nginx 反向代理监控

| 指标 | 说明 | 数据来源 |
|------|------|---------|
| 请求速率 | 每秒请求数（r/s） | Agent 采集 Nginx stub_status 或解析访问日志 |
| HTTP 状态码分布 | 2xx / 4xx / 5xx 占比 | 同上 |
| 活跃连接数 | active / reading / writing / waiting | 同上 |
| Upstream 健康 | 各反代后端的连通性与响应时间 | Agent 定时探测各 upstream 地址 |
| 带宽使用 | 上下行流量速率 | 同上 |

### 4. 📡 应用健康检查

| 功能 | 说明 |
|------|------|
| API 健康探针 | 定时调用各服务 `/api/v1/health` 端点（需 JWT 验证） |
| 依赖服务状态 | MySQL / Redis / MongoDB 连通性 + 深度指标（见下表） |
| 延迟监控 | 各服务响应时间统计 |
| 服务心跳 | 每个子项目的心跳检测 |

##### 应用健康检查列表（规划）

| 服务 | 内网端点 | 认证方式 | 检查内容 |
|------|---------|---------|---------|
| Auth 鉴权 | `http://auth:8100/api/v1/auth/health` | Bearer \<service-token\> | 服务存活、数据库连接 |
| Monitor API | `http://monitor-api:8051/api/v1/health` | Bearer \<service-token\> | 服务存活、MySQL/Redis 连接 |
| Notification | `http://notification:8110/api/v1/health` | Bearer \<service-token\> | 服务存活、数据库连接 |

> 应用健康检查在内网进行，使用 Monitor 服务的 Client Credentials JWT 调用。与站点监控的公开 `/healthz` 不同，这里是带权限的深度检测。|

#### 依赖服务深度指标

| 数据库 | 指标 | 说明 |
|--------|------|------|
| **MySQL** | 连接数 / 慢查询 / QPS / InnoDB 行锁 / 主从延迟 | 反映数据库健康与性能瓶颈 |
| **Redis** | 内存使用率 / 命中率 / key 总数 / 过期率 / 连接数 | 缓存效率与内存预警 |
| **MongoDB** | 连接数 / 读写延迟 / 存储大小 / opcounters | 文档数据库负载状态 |

### 5. 🔔 告警通知

| 功能 | 说明 |
|------|------|
| 告警规则 | 阈值设置（如 CPU > 90% 持续 5min） |
| 通知渠道 | 邮件 / 企业微信 / 钉钉 |
| 告警历史 | 触发时间、恢复时间、处理状态 |
| 静默期 | 避免重复告警轰炸 |

### 6. 🔄 CI/CD 流水线监控

通过 GitHub REST API 拉取工作流运行记录，追踪部署状态与构建趋势。

| 指标 | 展示方式 | 数据来源 |
|------|---------|---------|
| 最近部署列表 | 卡片列表（状态/时长/commit/分支） | Agent 每 5min 轮询 GitHub Actions API |
| 部署成功率 | 30 天聚合统计卡片 | 汇总历史运行记录 |
| 构建时长趋势 | 折线图（每次部署耗时） | 同上 |
| 失败原因摘要 | 列表（失败步骤日志简述） | GitHub API 返回的 conclusion + logs |
| 部署频率 | 周/月部署次数统计 | 同上 |
| 上次成功部署 | 时间戳 + commit SHA 展示 | 最后一次 successful run |

**GitHub Token 配置**：需创建 Personal Access Token（`repo` 权限），存入 `.env` 文件：
```
GITHUB_TOKEN=ghp_xxxxxxxxxxxx
GITHUB_REPO=Lenceas/Jason-hub
```

---

## 三、技术选型

| 模块 | 技术 | 说明 |
|------|------|------|
| 前端框架 | **Vue 3 + TypeScript** | Composition API，项目统一标准 |
| 样式方案 | **UnoCSS + Portfolio CSS 变量** | 零运行时按需生成，继承全站蓝白主题 |
| 状态管理 | **Pinia** | Vue 3 官方推荐 |
| 路由 | **Vue Router 4** | History 路由（干净 URL，Nginx fallback `try_files`） |
| 图表 | **ECharts** | 监控场景功能最全 |
| HTTP 客户端 | **axios** | - |
| 构建 | **Vite** | Vue 3 标配 |
| 后端框架 | **.NET 10 + Minimal API** | 轻量高性能 |
| API 文档 | **Scalar** | 集成至 `/api/v1/docs`，蓝白主题适配 |
| ORM | **SqlSugar** | 支持多数据库、多租户、国产数据库、MongoDB |
| 数据库 | **MySQL 8.4** | 使用独立 `monitor` 数据库，与 `auth` 用户中心隔离 |
| 缓存 | **Redis** | 实时数据和 Pub/Sub |
| 采集代理 | **.NET 10 Background Worker** | 服务器端定时采集 |
| Docker 通信 | **Docker SDK for .NET** | 容器状态获取 |

---

## 四、前端结构

```
Monitor/web/
├── src/
│   ├── layouts/
│   │   └── DashboardLayout.vue      ← 左侧导航 + 右侧内容区
│   ├── views/
│   │   ├── Overview.vue             ← 概览大盘（所有关键指标汇总）
│   │   ├── Server.vue               ← 服务器监控
│   │   ├── Docker.vue               ← Docker 容器
│   │   ├── Uptime.vue               ← 站点可用性
│   │   ├── Health.vue               ← 应用健康检查
│   │   ├── Alerts.vue               ← 告警规则 + 历史
│   │   └── Settings.vue             ← 通知渠道配置
│   ├── components/
│   │   ├── MetricsCard.vue          ← 指标数字卡片
│   │   ├── LineChart.vue            ← 趋势折线图（ECharts 封装）
│   │   ├── GaugeChart.vue           ← 仪表盘环图
│   │   ├── StatusBadge.vue          ← 状态标签（正常/告警/离线）
│   │   ├── AlertTable.vue           ← 告警列表
│   │   └── Timeline.vue             ← 时间轴（事件日志）
│   ├── stores/                      ← Pinia
│   ├── api/                         ← axios 封装
│   ├── styles/
│   │   └── variables.css            ← 继承 Portfolio CSS 变量 + 监控状态色
│   ├── types/                       ← TypeScript 类型定义
│   ├── App.vue
│   └── main.ts
├── index.html
├── vite.config.ts
├── Dockerfile
├── nginx.conf
└── package.json
```

### 配色方案

```css
/* 继承 Portfolio global.css */
--color-primary: #3B82F6;
--color-primary-light: #60A5FA;
--color-bg: #FFFFFF;
--color-bg-alt: #F8FAFC;

/* 监控专用状态色 */
--color-success: #10B981;    /* 正常/可用 */
--color-warning: #F59E0B;    /* 告警 */
--color-danger: #EF4444;     /* 故障 */
--color-info: #06B6D4;       /* 信息 */
```

---

## 五、API 设计

### 路由结构

```
GET    /api/v1/server/metrics           ← 实时服务器指标
GET    /api/v1/server/history?range=24h ← 历史指标

GET    /api/v1/docker/containers        ← 容器列表
GET    /api/v1/docker/containers/:id    ← 容器详情
POST   /api/v1/docker/containers/:id/restart

GET    /api/v1/uptime/sites             ← 站点列表
POST   /api/v1/uptime/sites             ← 新增监控站点
DELETE /api/v1/uptime/sites/:id         ← 删除站点
GET    /api/v1/uptime/sites/:id/history ← 可用性历史

GET    /api/v1/nginx/metrics            ← Nginx 实时指标（请求速率/状态码/连接数）
GET    /api/v1/nginx/history?range=24h  ← Nginx 历史趋势

GET    /api/v1/health/services          ← 所有服务健康状态
GET    /api/v1/health/databases         ← 数据库深度指标（MySQL/Redis/MongoDB）

GET    /api/v1/alerts/rules             ← 告警规则
POST   /api/v1/alerts/rules             ← 创建规则
PUT    /api/v1/alerts/rules/:id         ← 修改规则
DELETE /api/v1/alerts/rules/:id         ← 删除规则
GET    /api/v1/alerts/history           ← 告警历史

GET    /api/v1/channels                 ← 通知渠道
POST   /api/v1/channels                ← 配置渠道
PUT    /api/v1/channels/:id             ← 修改渠道

GET    /api/v1/ci/deployments           ← 最近部署列表（状态/时长/commit/分支）
GET    /api/v1/ci/stats                 ← 聚合统计（失败率/平均时长/部署频率）
GET    /api/v1/ci/last                  ← 最后一次部署详情

GET    /api/v1/docs                     ← Scalar API 文档
```

---

## 六、数据采集方案

服务器上部署一个 **.NET 10 Worker Service**（轻量后台服务）：

```
┌─────────────────────────────────────────────┐
│  Monitor.Agent (.NET Background Worker)     │
│  ───────────────────────────────────────    │
│  • 每 10s 采集 CPU/内存/磁盘/Docker 存储    │
│  • 每 30s 采集 Docker 容器状态              │
│  • 每 30s 采集 Nginx 指标（stub_status）    │
│  • 每 60s 做站点 HTTP 探测                  │
│  • 每 60s 采集数据库深度指标                 │
│  • 每 30s 调用各服务健康端点                 │
│  • 每 5min 轮询 GitHub Actions 部署状态     │
│                                             │
│  实时数据 → Redis（过期 1h）                  │
│  历史数据 → MySQL（保留 30d）                │
└─────────────────────────────────────────────┘
```

---

## 七、数据库设计

Monitor 使用独立 `jason_monitor` 数据库，与全局 `jason_auth` 用户中心隔离。告警事件中的用户信息通过 `user_id` 引用 `jason_auth.auth_users`。

### 表结构

```sql
-- 服务器指标表（每分钟一条记录）
CREATE TABLE server_metrics (
    id          BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '记录ID',
    ts          DATETIME NOT NULL                    COMMENT '采集时间戳',
    cpu_pct     DECIMAL(5,2)                         COMMENT 'CPU使用率（%）',
    mem_pct     DECIMAL(5,2)                         COMMENT '内存使用率（%）',
    mem_used    BIGINT                                COMMENT '已用内存（字节）',
    mem_total   BIGINT                                COMMENT '总内存（字节）',
    disk_pct    DECIMAL(5,2)                         COMMENT '磁盘使用率（%）',
    net_in      BIGINT                                COMMENT '入网流量（字节）',
    net_out     BIGINT                                COMMENT '出网流量（字节）',
    load_1m     DECIMAL(4,2)                         COMMENT '1分钟负载',
    load_5m     DECIMAL(4,2)                         COMMENT '5分钟负载',
    load_15m    DECIMAL(4,2)                         COMMENT '15分钟负载',
    INDEX idx_ts (ts)
) COMMENT='服务器指标表';

-- 站点配置表
CREATE TABLE sites (
    id           INT PRIMARY KEY AUTO_INCREMENT    COMMENT '站点ID',
    name         VARCHAR(100) NOT NULL              COMMENT '站点名称',
    url          VARCHAR(500) NOT NULL              COMMENT '站点URL',
    interval_sec INT DEFAULT 60                    COMMENT '探测间隔（秒）',
    timeout_ms   INT DEFAULT 5000                  COMMENT '超时时间（毫秒）',
    method       VARCHAR(10) DEFAULT 'GET'          COMMENT 'HTTP方法',
    is_active    TINYINT(1) DEFAULT 1              COMMENT '是否启用',
    created_at   DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间'
) COMMENT='站点配置表';

-- 站点检查记录表
CREATE TABLE uptime_records (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '记录ID',
    site_id      INT NOT NULL                      COMMENT '站点ID（关联sites）',
    status_code  INT                                COMMENT 'HTTP状态码',
    response_ms  INT                                COMMENT '响应时间（毫秒）',
    checked_at   DATETIME NOT NULL                 COMMENT '检查时间',
    is_ok        TINYINT(1) NOT NULL               COMMENT '是否正常',
    FOREIGN KEY (site_id) REFERENCES sites(id),
    INDEX idx_site_checked (site_id, checked_at)
) COMMENT='站点检查记录表';

-- 容器快照表
CREATE TABLE container_snapshots (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '记录ID',
    ts           DATETIME NOT NULL                 COMMENT '采集时间戳',
    name         VARCHAR(200) NOT NULL              COMMENT '容器名称',
    status       VARCHAR(50)                        COMMENT '容器状态',
    cpu_pct      DECIMAL(10,2)                     COMMENT 'CPU使用率',
    mem_usage    BIGINT                             COMMENT '内存使用量（字节）',
    mem_limit    BIGINT                             COMMENT '内存限制（字节）',
    INDEX idx_ts (ts)
) COMMENT='容器快照表';

-- 健康检查记录表
CREATE TABLE health_records (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '记录ID',
    ts           DATETIME NOT NULL                 COMMENT '检查时间',
    service      VARCHAR(100) NOT NULL              COMMENT '服务名称',
    endpoint     VARCHAR(500)                       COMMENT '检查端点',
    status       VARCHAR(20)                        COMMENT '状态码/结果',
    latency_ms   INT                                COMMENT '延迟（毫秒）',
    INDEX idx_service_ts (service, ts)
) COMMENT='健康检查记录表';

-- 告警规则表
CREATE TABLE alert_rules (
    id           INT PRIMARY KEY AUTO_INCREMENT    COMMENT '规则ID',
    name         VARCHAR(200) NOT NULL              COMMENT '规则名称',
    metric       VARCHAR(100) NOT NULL              COMMENT '监控指标名',
    operator     VARCHAR(10) NOT NULL               COMMENT '比较运算符（> / < / >= / <=）',
    threshold    DECIMAL(10,2) NOT NULL             COMMENT '告警阈值',
    duration_sec INT DEFAULT 300                   COMMENT '持续时长（秒）',
    enabled      TINYINT(1) DEFAULT 1              COMMENT '是否启用',
    created_at   DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间'
) COMMENT='告警规则表';

-- 告警事件表
CREATE TABLE alert_events (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '事件ID',
    rule_id      INT NOT NULL                      COMMENT '关联规则ID',
    triggered_at DATETIME NOT NULL                 COMMENT '触发时间',
    resolved_at  DATETIME                          COMMENT '恢复时间',
    message      TEXT                               COMMENT '告警消息',
    severity     VARCHAR(20) DEFAULT 'warning'     COMMENT '严重级别（warning/critical）',
    FOREIGN KEY (rule_id) REFERENCES alert_rules(id),
    INDEX idx_rule_triggered (rule_id, triggered_at)
) COMMENT='告警事件表';
```

---

## 八、项目目录结构

```
Jason-hub/
├── Monitor/
│   ├── web/                    ← Vue 3 + TypeScript 前端
│   │   ├── src/
│   │   ├── Dockerfile
│   │   ├── nginx.conf
│   │   └── package.json
│   └── api/                    ← .NET 10 + SqlSugar 后端
│       ├── Controllers/
│       ├── Services/
│       ├── Models/
│       ├── Data/
│       ├── Worker/              ← Background Service 采集器
│       └── Dockerfile
├── docker-compose.yml          ← 追加 monitor-web + monitor-api
└── Portfolio/src/data/projects.json  ← 追加 Monitor 卡片
```

---

## 九、部署检查清单

按 [DEPLOY.md](../DEPLOY.md) 规范完成：

- [ ] 创建 Monitor/web + Monitor/api 项目骨架
- [ ] 编写前端 Dockerfile + nginx.conf
- [ ] 编写后端 Dockerfile
- [ ] `docker-compose.yml` 追加 monitor-web（:8001）+ monitor-api（:8051）
- [ ] GitHub Actions `deploy.yml` 增加新 service 构建命令
- [ ] DNS 添加 `monitor.lujiesheng.cn` + `api-monitor.lujiesheng.cn` → `81.71.136.3`
- [ ] SSL：acme.sh 申请子域名证书
- [ ] Nginx 添加子域名 server 块
- [ ] Portfolio `projects.json` 添加 Monitor 卡片
- [ ] 更新各 MD 文档

---

> **本方案于 2026-05-30 经用户确认，后续开发严格按此执行。如有调整需更新本文档并重新确认。**
