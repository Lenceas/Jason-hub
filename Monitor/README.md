# Monitor

> [中文](#zh) · [English](#en)

---

<h2 id="zh">中文</h2>

Jason-hub 统一监控平台 · 子域名 `monitor.lujiesheng.cn`

服务器、Docker 容器、站点可用性、应用健康的实时监控与告警系统。

### 技术栈

| 模块 | 技术 |
|------|------|
| 前端 | Vue 3 + TypeScript + UnoCSS + ECharts + Pinia |
| 后端 | .NET 10 + SqlSugar + Scalar |
| 数据 | MySQL 8.4 + Redis 8 |
| 部署 | Docker（端口 8001 / 8051） |

### 目录结构

```
Monitor/
├── web/          ← Vue 3 前端（开发中）
│   ├── src/
│   │   ├── views/       ← 7 个页面
│   │   ├── components/  ← 6 个通用组件
│   │   ├── stores/      ← Pinia
│   │   ├── api/         ← axios 封装
│   │   └── types/       ← TS 类型
│   ├── Dockerfile
│   ├── nginx.conf
│   └── package.json
├── api/          ← .NET 10 后端（开发中）
│   ├── Endpoints/       ← API 端点
│   ├── Services/        ← 业务服务
│   ├── Models/          ← DTO + 实体
│   ├── Worker/          ← 后台采集服务
│   └── Dockerfile
├── Shared/       ← 共享库
├── PLAN.md       ← 方案设计文档
├── CHANGELOG.md  ← 版本变更日志
└── README.md     ← 技术说明（本文件）
```

### 开发命令

```bash
# 前端 → 详见 [web/README.md](./web/README.md)
cd Monitor/web
npm install
npm run dev          # http://localhost:8001

# 后端 → 详见 [api/README.md](./api/README.md)
cd Monitor/api
dotnet restore
dotnet run           # http://localhost:8051
```

### 相关文档

- [方案设计](./PLAN.md) — 功能范围、API 设计、数据库设计
- [变更日志](./CHANGELOG.md) — 版本历史

---

<h2 id="en">English</h2>

Jason-hub Unified Monitoring Platform · Subdomain `monitor.lujiesheng.cn`

Real-time monitoring and alerting system for server metrics, Docker containers, site uptime, and application health.

### Tech Stack

| Module | Technology |
|--------|-----------|
| Frontend | Vue 3 + TypeScript + UnoCSS + ECharts + Pinia |
| Backend | .NET 10 + SqlSugar + Scalar |
| Data | MySQL 8.4 + Redis 8 |
| Deployment | Docker (port 8001 / 8051) |

### Directory Structure

```
Monitor/
├── web/          ← Vue 3 frontend (planned)
│   ├── src/
│   │   ├── views/       ← 7 pages
│   │   ├── components/  ← 6 reusable components
│   │   ├── stores/      ← Pinia
│   │   ├── api/         ← axios
│   │   └── types/       ← TS type definitions
│   ├── Dockerfile
│   ├── nginx.conf
│   └── package.json
├── api/          ← .NET 10 backend (planned)
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── Data/
│   └── Worker/         ← Background collection service
├── PLAN.md       ← Design document (locked after approval)
├── CHANGELOG.md  ← Version changelog
└── README.md     ← Technical guide (this file)
```

### Development Commands

```bash
# Frontend
cd Monitor/web
npm install
npm run dev          # Dev server → http://localhost:8001

# Backend
cd Monitor/api
dotnet restore
dotnet run           # → http://localhost:8051
```

### Related Docs

- [Design Plan](./PLAN.md) — Feature scope, API design, database schema
- [Changelog](./CHANGELOG.md) — Version history
