# Jason-hub

> [中文](#zh) · [English](#en)

---

<h2 id="zh">中文</h2>

个人项目 Monorepo · 主站聚合 · 子项目统一入口

通过 [Portfolio](./Portfolio) 主站聚合展示个人介绍与项目卡片，跳转到各个独立子项目。

### 结构

```
Jason-hub/
├── Portfolio/     ← Astro 6 个人主页（主站）
├── ...            ← 后续子项目 (Vue 3 + TS)
├── AGENTS.md      ← AI 协作说明
├── ARCHITECTURE.md ← 架构设计
└── STYLE_GUIDE.md ← 编码规范
```

### 端口规则

| 类型 | 范围 |
|------|------|
| 前端开发 | 8000–8049 |
| 后端 API | 8050–8099 |

Portfolio 主站使用 **8000** 端口。

### 分支与版本

- **main** — 主分支，始终可部署
- **project/\<name\>** — 子项目开发分支
- 版本号：`v1.0.0` → `v1.1.0` → `v1.1.1` ...

详见 [STYLE_GUIDE.md](./STYLE_GUIDE.md#git-分支与版本规范) Git 章节。

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

A personal project aggregation site built with [Astro 6](./Portfolio). The main page showcases a personal profile and project cards that link to independent sub-projects.

### Structure

```
Jason-hub/
├── Portfolio/     ← Astro 6 personal homepage (main site)
├── ...            ← Upcoming sub-projects (Vue 3 + TS)
├── AGENTS.md      ← AI agent instructions
├── ARCHITECTURE.md ← Architecture design
└── STYLE_GUIDE.md ← Coding standards
```

### Port Convention

| Type | Range |
|------|-------|
| Frontend | 8000–8049 |
| Backend API | 8050–8099 |

Portfolio runs on port **8000**.

### Branching & Versioning

- **main** — production-ready at all times
- **project/\<name\>** — sub-project development branches
- Versioning: `v1.0.0` → `v1.1.0` → `v1.1.1` ...

See [STYLE_GUIDE.md](./STYLE_GUIDE.md#git-分支与版本规范) for details.

### Quick Start

```bash
cd Portfolio
npm install
npm run dev    # → http://localhost:8000
npm run build
```

---

参见：[AGENTS.md](./AGENTS.md) · [ARCHITECTURE.md](./ARCHITECTURE.md) · [STYLE_GUIDE.md](./STYLE_GUIDE.md)

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
