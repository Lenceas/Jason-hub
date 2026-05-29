# Jason-hub

> [中文](#zh) · [English](#en)

---

<h2 id="zh">中文</h2>

个人项目 Monorepo · 主站聚合 · 子项目统一入口

通过 [Portfolio](./Portfolio) 主站聚合展示个人介绍、技能标签、项目卡片，作为所有子项目的统一入口。

### 在线地址

**https://lujiesheng.cn**

### 子项目

| 项目 | 前端 | API | 技术栈 | 状态 |
|------|------|----|--------|------|
| Portfolio 主站 | `lujiesheng.cn` | — | Astro 6 | active |
| 后续项目 | `<name>.lujiesheng.cn` | `api-<name>.lujiesheng.cn` | Vue 3 / .NET 10 | planning |

### 结构

```
Jason-hub/
├── Portfolio/           ← Astro 6 个人主页（主站）
├── project-<name>/      ← 后续子项目 (Vue 3 + .NET 10)
├── .github/workflows/   ← GitHub Actions CI/CD
├── docker-compose.yml   ← Docker 容器编排
├── AGENTS.md            ← AI 协作说明
├── ARCHITECTURE.md      ← 架构设计
├── CHANGELOG.md         ← Monorepo 更新日志
├── DEPLOY.md            ← 部署规范（Nginx / SSL / CI-CD）
├── RELEASE.md           ← 发布工作流
├── STYLE_GUIDE.md       ← 编码与 Git 规范
└── README.md            ← 项目总览（本文件）
```

### 快速开始

```bash
cd Portfolio
npm install
npm run dev    # → http://localhost:8000
npm run build
```

---

<h2 id="en">English</h2>

Personal Monorepo — Central hub & sub-project entry point

A personal project aggregation site built with [Astro 6](./Portfolio). The main page showcases a personal profile, skills, and project cards that link to independent sub-projects.

### Live Site

**https://lujiesheng.cn**

### Sub-Projects

| Project | Frontend | API | Stack | Status |
|---------|----------|-----|-------|--------|
| Portfolio | `lujiesheng.cn` | — | Astro 6 | active |
| Upcoming | `<name>.lujiesheng.cn` | `api-<name>.lujiesheng.cn` | Vue 3 / .NET 10 | planning |

### Structure

```
Jason-hub/
├── Portfolio/           ← Astro 6 homepage (main site)
├── project-<name>/      ← Upcoming sub-projects (Vue 3 + .NET 10)
├── .github/workflows/   ← GitHub Actions CI/CD
├── docker-compose.yml   ← Docker container orchestration
├── AGENTS.md            ← AI agent instructions
├── ARCHITECTURE.md      ← Architecture design
├── CHANGELOG.md         ← Monorepo changelog
├── DEPLOY.md            ← Deployment guide (Nginx / SSL / CI-CD)
├── RELEASE.md           ← Release workflow
├── STYLE_GUIDE.md       ← Coding & Git standards
└── README.md            ← Project overview (this file)
```

### Quick Start

```bash
cd Portfolio
npm install
npm run dev    # → http://localhost:8000
npm run build
```

---

参见：[AGENTS.md](./AGENTS.md) · [ARCHITECTURE.md](./ARCHITECTURE.md) · [DEPLOY.md](./DEPLOY.md) · [STYLE_GUIDE.md](./STYLE_GUIDE.md)

---

## 许可证

本项目采用 **MIT** 协议开源，仅覆盖代码部分。

**以下内容不在此协议范围内，保留所有权利：**
- 个人介绍、头像、照片
- 博客文章、笔记等内容

详见 [LICENSE](./LICENSE)。

## License

This project is open-sourced under the **MIT** license, covering only the source code.

**The following are NOT covered and all rights are reserved:**
- Personal bio, avatar, photographs
- Blog posts, notes, and written content

See [LICENSE](./LICENSE) for details.
