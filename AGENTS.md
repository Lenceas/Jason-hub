# AGENTS.md

> 本文件用于向 AI 编程助手描述 Jason-hub 项目的整体上下文、技术栈、目录结构及开发规范。

## 项目概述

Jason-hub 是一个个人项目聚合站，采用 Monorepo 结构管理所有子项目。主站是一个基于 Astro 构建的极简个人主页（Portfolio），展示个人信息与项目卡片，通过卡片跳转到各个独立子项目。

## 技术栈

| 层级 | 技术 | 说明 |
|------|------|------|
| 主站框架 | **Astro 6** | 静态站点生成，默认零 JS 运行时 |
| 样式方案 | **原生 CSS + CSS 自定义属性** | 极简清爽，蓝白配色，三端响应式 |
| 子项目框架 | **Vue 3 + TypeScript** | 后续各子项目的统一前端技术栈 |
| 子项目后端 | **.NET 10 + Minimal API** | 统一后端 API 技术栈，配套 SqlSugar ORM |
| API 文档 | **Scalar** | 集成至 `/api/v1/docs`，蓝白主题适配 |
| 包管理 | **npm** | |

## 目录结构

```
Jason-hub/
├── Portfolio/              ← Astro 主站（个人介绍 + 项目卡片）
│   ├── public/images/      ← 静态资源（头像、Logo、项目封面）
│   ├── src/
│   │   ├── pages/          ← 页面路由（单页：index.astro）
│   │   ├── layouts/        ← 布局模板（BaseLayout）
│   │   ├── components/     ← 10 个组件
│   │   ├── data/           ← 项目数据（JSON）
│   │   └── styles/         ← 全局样式（CSS 变量 + Reset + 响应式）
│   ├── PLAN.md             ← 方案设计
│   ├── CHANGELOG.md        ← 变更日志
│   ├── README.md           ← 技术说明（中英双语）
│   ├── Dockerfile
│   ├── nginx.conf
│   ├── astro.config.mjs
│   └── package.json
├── Monitor/                ← Vue 3 + .NET 10 监控面板（development）
│   ├── PLAN.md             ← 方案设计
│   ├── CHANGELOG.md        ← 变更日志
│   └── README.md           ← 技术说明（中英双语）
├── .github/workflows/      ← GitHub Actions CI/CD
├── docker-compose.yml      ← Docker 容器编排
├── .env.example            ← 环境变量模板
├── PLAN.md                 ← Monorepo 总体规划
├── CHANGELOG.md            ← 主仓库变更日志
├── CLAUDE.md               ← 会话记忆（自动维护）
├── README.md               ← 项目总览（中英双语）
├── DEPLOY.md               ← 部署规范
├── RELEASE.md              ← 发布工作流
├── ARCHITECTURE.md         ← 整体架构设计
├── AGENTS.md               ← AI 通用说明（本文件）
├── STYLE_GUIDE.md          ← 编码规范
└── LICENSE                 ← MIT 协议
```

## 开发规范

- **主站（Portfolio）**：Astro 6 + 原生 CSS，保持轻量，全端响应式
- **子项目**：Vue 3 + TypeScript
- **添加新子项目**时，同步更新 `Portfolio/src/data/projects.json` 添加对应项目卡片
- **项目卡片字段**：`id`、`title`、`tagline`、`description`、`status`（active/planning/archived/completed）、`url`、`thumbnail`、`tags`、`year`

## Git 分支规范

采用简化版 GitHub Flow（单人适用）。

| 分支 | 用途 | 说明 |
|------|------|------|
| `main` | 主分支 | 始终可部署，合并即发布 |
| `project/<name>` | 子项目开发 | 如 `project/auth`，长期存在，持续迭代分支；开发里程碑合并到 main 后保留 |
| `feat/<描述>` | 功能开发 | 可选，小功能独立分支 |
| `fix/<描述>` | 修复 | Bug 修复 |

**版本号**（语义化版本）：
- `v1.0.0` — 首个可发布版本
- `v1.1.0` — 新增子项目 / 新功能
- `v1.1.1` — Bug 修复

更多细节见 [STYLE_GUIDE.md](./STYLE_GUIDE.md) Git 章节。

## 端口规则

所有端口仅绑定 `127.0.0.1`，外网通过 Nginx 访问。

| 类型 | 端口范围 | 说明 |
|------|----------|------|
| 前端页面 | 8000–8049 | Astro 主站 / Vue 子项目 |
| 子项目 API | 8050–8099 | 子项目对应的 API 服务 |
| 基础设施服务 | 8100–8149 | Auth 鉴权 / 通知 / 任务调度 / 消息队列等 |
| 数据库 | 标准端口 | MySQL 3306 / Redis 6379 / MongoDB 27017 |

### 端口分配表

| 项目 | 类型 | 端口 | 域名 |
|------|------|------|------|
| **Portfolio** | Astro 前端 | 8000 | `lujiesheng.cn` |
| **Monitor** | Vue 3 前端 | 8001 | `monitor.lujiesheng.cn` |
| Monitor API | .NET 10 后端（含 Agent 采集器） | 8051 | `api-monitor.lujiesheng.cn` |
| **Auth 服务** | .NET 10 鉴权中心（JWT） | 8100 | `api-auth.lujiesheng.cn` |
| **Notification** | .NET 10 通知服务 | 8110 | `api-notification.lujiesheng.cn` |
| **任务调度** | .NET 10 任务调度服务 | 8120 | —（内网） |
| 消息队列 | 基础设施服务 | 8130 | —（内网） |
| 子项目 N | Vue 3 前端 | 8002+ | `<name>.lujiesheng.cn` |
| 子项目 N API | .NET 10 后端 | 8052+ | `api-<name>.lujiesheng.cn` |

> 新增子项目时按此规则分配端口，并更新本表。

## 基础设施

MySQL 8.4 / Redis 8 / MongoDB 8 三数据库作为全局基础设施，所有子项目共用。容器内默认端口：`3306` / `6379` / `27017`。

## 部署与域名

| 项目 | 域名 | 说明 |
|------|------|------|
| Portfolio | `lujiesheng.cn` `www.lujiesheng.cn` | 主入口 |
| Monitor | `monitor.lujiesheng.cn` | 第一个子项目 |
| 子项目前端 | `<name>.lujiesheng.cn` | 后续子项目 |
| 子项目 API | `api-<name>.lujiesheng.cn` | 子项目后端 |

- SSL 证书：acme.sh + Let's Encrypt + 腾讯云 DNS API，一个域名一张免费证书
- 部署：GitHub Actions → SCP 上传代码 → `docker compose up --build -d`
- Nginx：主机 80/443 → 反代到 `127.0.0.1:<port>`
- 详情见 [DEPLOY.md](./DEPLOY.md)

## 子项目列表

| 子项目 | 技术栈 | 状态 | 说明 |
|--------|--------|------|------|
| Portfolio | Astro 6 | active | 个人主页主站 |
| Monitor | Vue 3 + .NET 10 | development | 监控面板，开发中，详见 [Monitor/PLAN.md](./Monitor/PLAN.md) |
