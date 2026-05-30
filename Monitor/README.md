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

### 4. 📡 应用健康检查

| 功能 | 说明 |
|------|------|
| API 健康探针 | 定时调用各服务 `/health` 端点 |
| 依赖服务状态 | MySQL / Redis / MongoDB 连通性 |
| 延迟监控 | 各服务响应时间统计 |
| 服务心跳 | 每个子项目的心跳检测 |

### 5. 🔔 告警通知

| 功能 | 说明 |
|------|------|
| 告警规则 | 阈值设置（如 CPU > 90% 持续 5min） |
| 通知渠道 | 邮件 / 企业微信 / 钉钉 |
| 告警历史 | 触发时间、恢复时间、处理状态 |
| 静默期 | 避免重复告警轰炸 |

---

## 三、技术选型

| 模块 | 技术 | 说明 |
|------|------|------|
| 前端框架 | **Vue 3 + TypeScript** | Composition API，项目统一标准 |
| 样式方案 | **UnoCSS + Portfolio CSS 变量** | 零运行时按需生成，继承全站蓝白主题 |
| 状态管理 | **Pinia** | Vue 3 官方推荐 |
| 路由 | **Vue Router 4** | Hash 路由（纯前端 SPA） |
| 图表 | **ECharts** | 监控场景功能最全 |
| HTTP 客户端 | **axios** | - |
| 构建 | **Vite** | Vue 3 标配 |
| 后端框架 | **.NET 10 + Minimal API** | 轻量高性能 |
| API 文档 | **Scalar** | 集成至 `/api/v1/docs`，蓝白主题适配 |
| ORM | **SqlSugar** | 支持多数据库、多租户、国产数据库、MongoDB |
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

GET    /api/v1/health/services          ← 所有服务健康状态

GET    /api/v1/alerts/rules             ← 告警规则
POST   /api/v1/alerts/rules             ← 创建规则
PUT    /api/v1/alerts/rules/:id         ← 修改规则
DELETE /api/v1/alerts/rules/:id         ← 删除规则
GET    /api/v1/alerts/history           ← 告警历史

GET    /api/v1/channels                 ← 通知渠道
POST   /api/v1/channels                ← 配置渠道
PUT    /api/v1/channels/:id             ← 修改渠道

GET    /api/v1/docs                     ← Scalar API 文档
```

---

## 六、数据采集方案

服务器上部署一个 **.NET 10 Worker Service**（轻量后台服务）：

```
┌─────────────────────────────────────────┐
│  Monitor.Agent (.NET Background Worker) │
│  ─────────────────────────────────────  │
│  • 每 10s 采集 CPU/内存/磁盘/网络       │
│  • 每 30s 采集 Docker 容器状态          │
│  • 每 60s 做站点 HTTP 探测              │
│  • 每 30s 调用各服务健康端点             │
│                                         │
│  实时数据 → Redis（过期 1h）              │
│  历史数据 → MySQL（保留 30d）            │
└─────────────────────────────────────────┘
```

---

## 七、数据库设计

### 表结构

```sql
-- 服务器指标（每分钟一条）
CREATE TABLE server_metrics (
    id          BIGINT PRIMARY KEY AUTO_INCREMENT,
    ts          DATETIME NOT NULL,
    cpu_pct     DECIMAL(5,2),
    mem_pct     DECIMAL(5,2),
    mem_used    BIGINT,
    mem_total   BIGINT,
    disk_pct    DECIMAL(5,2),
    net_in      BIGINT,
    net_out     BIGINT,
    load_1m     DECIMAL(4,2),
    load_5m     DECIMAL(4,2),
    load_15m    DECIMAL(4,2),
    INDEX idx_ts (ts)
);

-- 站点配置
CREATE TABLE sites (
    id           INT PRIMARY KEY AUTO_INCREMENT,
    name         VARCHAR(100) NOT NULL,
    url          VARCHAR(500) NOT NULL,
    interval_sec INT DEFAULT 60,
    timeout_ms   INT DEFAULT 5000,
    method       VARCHAR(10) DEFAULT 'GET',
    is_active    TINYINT(1) DEFAULT 1,
    created_at   DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 站点检查记录
CREATE TABLE uptime_records (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT,
    site_id      INT NOT NULL,
    status_code  INT,
    response_ms  INT,
    checked_at   DATETIME NOT NULL,
    is_ok        TINYINT(1) NOT NULL,
    FOREIGN KEY (site_id) REFERENCES sites(id),
    INDEX idx_site_checked (site_id, checked_at)
);

-- 容器快照
CREATE TABLE container_snapshots (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT,
    ts           DATETIME NOT NULL,
    name         VARCHAR(200) NOT NULL,
    status       VARCHAR(50),
    cpu_pct      DECIMAL(10,2),
    mem_usage    BIGINT,
    mem_limit    BIGINT,
    INDEX idx_ts (ts)
);

-- 健康检查记录
CREATE TABLE health_records (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT,
    ts           DATETIME NOT NULL,
    service      VARCHAR(100) NOT NULL,
    endpoint     VARCHAR(500),
    status       VARCHAR(20),
    latency_ms   INT,
    INDEX idx_service_ts (service, ts)
);

-- 告警规则
CREATE TABLE alert_rules (
    id           INT PRIMARY KEY AUTO_INCREMENT,
    name         VARCHAR(200) NOT NULL,
    metric       VARCHAR(100) NOT NULL,
    operator     VARCHAR(10) NOT NULL,
    threshold    DECIMAL(10,2) NOT NULL,
    duration_sec INT DEFAULT 300,
    enabled      TINYINT(1) DEFAULT 1,
    created_at   DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 告警事件
CREATE TABLE alert_events (
    id           BIGINT PRIMARY KEY AUTO_INCREMENT,
    rule_id      INT NOT NULL,
    triggered_at DATETIME NOT NULL,
    resolved_at  DATETIME,
    message      TEXT,
    severity     VARCHAR(20) DEFAULT 'warning',
    FOREIGN KEY (rule_id) REFERENCES alert_rules(id),
    INDEX idx_rule_triggered (rule_id, triggered_at)
);
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
