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
                     └──┬───┬───┬──┘
                        │   │   │
              ┌─────────┘   │   └──────────┐
              ▼             ▼              ▼
       ┌──────────┐ ┌──────────┐ ┌──────────┐
       │ Portfolio │ │ Monitor  │ │ 子项目N   │ ← Vue 3 前端
       │ :8000     │ │ :8001    │ │ :8002+    │
       │ Astro 6   │ │ Vue 3    │ │ Vue 3     │
       └──────────┘ └────┬─────┘ └──────────┘
                         │
                         ▼
                  ┌──────────┐
                  │ Monitor  │ ← .NET 10 API
                  │ :8051    │
                  └──────────┘
                         │
              ┌──────────┴──────────┐
              │                     │
              ▼                     ▼
       ┌──────────┐         ┌──────────┐
       │  MySQL   │         │  Redis   │ ← 全局基础设施
       │  8.4     │         │  8       │
       └──────────┘         └──────────┘
```

### 层级说明

| 层级 | 说明 |
|------|------|
| **接入层** | Nginx 80/443 → SSL termination → 按域名反代到容器 |
| **展示层** | Portfolio（Astro 6 静态站）作为统一入口 |
| **应用层** | 各子项目独立容器部署（Vue 3 前端 + .NET 10 API） |
| **基础设施** | MySQL 8.4 + Redis 8 + MongoDB 8 全局共享 |

---

## 三、子项目生态

### 命名规范

| 类型 | 格式 | 示例 |
|------|------|------|
| 子项目目录 | PascalCase | `Monitor/` |
| 前端域名 | `<name>.lujiesheng.cn` | `monitor.lujiesheng.cn` |
| API 域名 | `api-<name>.lujiesheng.cn` | `api-monitor.lujiesheng.cn` |
| 前端端口 | 8000–8049 | 8001 |
| API 端口 | 8050–8099 | 8051 |

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

## 八、关键决策记录

| 日期 | 决策 | 参考 |
|------|------|------|
| 2026-05-30 | 子项目文档三件套：PLAN.md / CHANGELOG.md / README.md | `STYLE_GUIDE.md` |
| 2026-05-30 | 双轨发布流：子项目分支独立发版 + main 主发布流 | `RELEASE.md` |
| 2026-05-30 | 子项目独立版本号，main 合并时 +1 minor | `STYLE_GUIDE.md` |
| 2026-05-30 | 子项目统一 Vue 3 + .NET 10 | `ARCHITECTURE.md` |
| 2026-05-30 | Monitor 方案确定（5 大模块 + 7 张表 + Agent 采集） | `Monitor/PLAN.md` |
| 2026-05-29 | 基础设施层 MySQL 8.4 + Redis 8 + MongoDB 8 | `docker-compose.yml` |
| 2026-05-27 | Monorepo 五种命名规范 | `STYLE_GUIDE.md` |

---

> **本文档随项目发展持续更新，重大架构变更需同步修改。**
