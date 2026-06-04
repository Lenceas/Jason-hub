# Jason-hub

> [中文](#zh) · [English](#en)

---

<h2 id="zh">中文</h2>

个人项目 Monorepo · 主站聚合 · 子项目统一入口

通过 [Portfolio](./Portfolio) 主站聚合展示个人介绍、技能标签、项目卡片，作为所有子项目的统一入口。

### 在线地址

[**https://lujiesheng.cn**](https://lujiesheng.cn)

### 子项目

| 项目 | 前端 | API 文档 | 技术栈 | 状态 |
|------|------|---------|--------|------|
| Portfolio 主站 | [lujiesheng.cn](https://lujiesheng.cn) | — | Astro 6 | active |
| Auth 鉴权服务 | [登录页](https://api-auth.lujiesheng.cn/login) | [Scalar 文档](https://api-auth.lujiesheng.cn/scalar/v1) | .NET 10 / Minimal API | active |
| Monitor 监控面板 | `monitor.lujiesheng.cn` | `api-monitor.lujiesheng.cn` | Vue 3 / .NET 10 | development |
| 后续项目 | `<name>.lujiesheng.cn` | `api-<name>.lujiesheng.cn` | Vue 3 / .NET 10 | planning |

### 结构

```
Jason-hub/
├── Portfolio/           ← Astro 6 个人主页（主站）
├── Auth/                ← .NET 10 鉴权服务（基础设施）
├── Monitor/             ← Vue 3 + .NET 10 监控面板（development）
├── templates/           ← 项目脚手架模板
│   └── dotnet-service/  ← .NET 后端子项目模板
├── scripts/             ← 辅助脚本
│   └── scaffold-dotnet.sh ← .NET 后端子项目生成器
├── .github/workflows/   ← GitHub Actions CI/CD
├── docker-compose.yml   ← Docker 容器编排
├── .env.example         ← 环境变量模板
├── PLAN.md              ← Monorepo 总体规划
├── CHANGELOG.md         ← 主仓库变更日志
├── CLAUDE.md            ← 会话记忆（自动维护）
├── README.md            ← 项目总览（本文件）
├── DEPLOY.md            ← 部署规范（Nginx / SSL / CI-CD）
├── RELEASE.md           ← 发布工作流
├── ARCHITECTURE.md      ← 架构设计
├── AGENTS.md            ← AI 协作说明
├── STYLE_GUIDE.md       ← 编码与 Git 规范
└── LICENSE              ← MIT 协议
```

### 快速开始

```bash
cd Portfolio
npm install
npm run dev    # → http://localhost:8000
npm run build
```

### 许可证

本项目采用 **MIT** 协议开源，仅覆盖代码部分。

**以下内容不在此协议范围内，保留所有权利：**
- 个人介绍、头像、照片
- 博客文章、笔记等内容

详见 [LICENSE](./LICENSE)。

---

<h2 id="en">English</h2>

Personal Monorepo — Central hub & sub-project entry point

A personal project aggregation site built with [Astro 6](./Portfolio). The main page showcases a personal profile, skills, and project cards that link to independent sub-projects.

### Live Site

[**https://lujiesheng.cn**](https://lujiesheng.cn)

### Sub-Projects

| Project | Frontend | API Docs | Stack | Status |
|---------|----------|----------|-------|--------|
| Portfolio | [lujiesheng.cn](https://lujiesheng.cn) | — | Astro 6 | active |
| Auth | [Login](https://api-auth.lujiesheng.cn/login) | [Scalar Docs](https://api-auth.lujiesheng.cn/scalar/v1) | .NET 10 / Minimal API | active |
| Monitor | `monitor.lujiesheng.cn` | `api-monitor.lujiesheng.cn` | Vue 3 / .NET 10 | development |
| Upcoming | `<name>.lujiesheng.cn` | `api-<name>.lujiesheng.cn` | Vue 3 / .NET 10 | planning |

### Structure

```
Jason-hub/
├── Portfolio/           ← Astro 6 homepage (main site)
├── Auth/                ← .NET 10 auth service (infrastructure)
├── Monitor/             ← Vue 3 + .NET 10 monitoring dashboard (development)
├── templates/           ← Project scaffold templates
│   └── dotnet-service/  ← .NET backend sub-project template
├── scripts/             ← Utility scripts
│   └── scaffold-dotnet.sh ← .NET backend sub-project generator
├── .github/workflows/   ← GitHub Actions CI/CD
├── docker-compose.yml   ← Docker container orchestration
├── .env.example         ← Environment variable template
├── PLAN.md              ← Monorepo master plan
├── CHANGELOG.md         ← Main repo changelog
├── CLAUDE.md            ← Session memory (auto-maintained)
├── README.md            ← Project overview (this file)
├── DEPLOY.md            ← Deployment guide (Nginx / SSL / CI-CD)
├── RELEASE.md           ← Release workflow
├── ARCHITECTURE.md      ← Architecture design
├── AGENTS.md            ← AI agent instructions
├── STYLE_GUIDE.md       ← Coding & Git standards
└── LICENSE              ← MIT license
```

### Quick Start

```bash
cd Portfolio
npm install
npm run dev    # → http://localhost:8000
npm run build
```

### License

This project is open-sourced under the **MIT** license, covering only the source code.

**The following are NOT covered and all rights are reserved:**
- Personal bio, avatar, photographs
- Blog posts, notes, and written content

See [LICENSE](./LICENSE).

---

参见：[AGENTS.md](./AGENTS.md) · [ARCHITECTURE.md](./ARCHITECTURE.md) · [DEPLOY.md](./DEPLOY.md) · [STYLE_GUIDE.md](./STYLE_GUIDE.md)
