# AGENTS.md

> 本文件用于向 AI 编程助手描述 Jason-hub 项目的整体上下文、技术栈、目录结构及开发规范。

## 项目概述

Jason-hub 是一个个人项目聚合站，采用 Monorepo 结构管理所有子项目。主站是一个基于 Astro 构建的极简个人主页（Portfolio），展示个人信息与项目卡片，通过卡片跳转到各个独立子项目。

## 技术栈

| 层级 | 技术 | 说明 |
|------|------|------|
| 主站框架 | **Astro 6** | 静态站点生成，默认零 JS 运行时 |
| 样式方案 | **原生 CSS + CSS 自定义属性** | 极简清爽，蓝白配色，三端响应式 |
| 子项目框架 | **Vue 3 + TypeScript** | 后续各子项目的统一技术栈 |
| 包管理 | **npm** | |

## 目录结构

```
Jason-hub/
├── Portfolio/              ← Astro 主站（个人介绍 + 项目卡片）
│   ├── public/images/      ← 静态资源（头像、Logo、项目封面）
│   ├── src/
│   │   ├── pages/          ← 页面路由（单页：index.astro）
│   │   ├── layouts/        ← 布局模板（BaseLayout）
│   │   ├── components/     ← 组件（Header/Hero/ProjectCard/Grid/Footer）
│   │   ├── data/           ← 项目数据（JSON）
│   │   └── styles/         ← 全局样式（CSS 变量 + Reset + 响应式）
│   ├── astro.config.mjs
│   └── package.json
├── ... (后续子项目文件夹)
├── AGENTS.md               ← AI 通用说明（本文件）
├── ARCHITECTURE.md         ← 整体架构设计
├── README.md               ← 项目总览
└── STYLE_GUIDE.md          ← 编码规范
```

## 开发规范

- **主站（Portfolio）**：Astro 6 + 原生 CSS，保持轻量，全端响应式
- **子项目**：Vue 3 + TypeScript
- **添加新子项目**时，同步更新 `Portfolio/src/data/projects.json` 添加对应项目卡片
- **项目卡片字段**：`id`、`title`、`tagline`、`description`、`status`（active/planning/archived）、`url`、`thumbnail`、`tags`、`year`

## 端口规则（开发环境）

用于本地开发和反向代理配置。

| 类型 | 端口范围 | 说明 |
|------|----------|------|
| 前端页面 | 8000–8049 | Astro 主站 / Vue 子项目 |
| 后端 API | 8050–8099 | 子项目对应的 API 服务 |

### 端口分配表

| 项目 | 类型 | 端口 | 反向代理目标 |
|------|------|------|-------------|
| **Portfolio 主站** | Astro 前端 | **8000** | `portfolio.域名` |
| 子项目 1 | Vue 3 前端 | 8001 | `项目1.域名` |
| 子项目 2 | Vue 3 前端 | 8002 | `项目2.域名` |
| ... | ... | 依次递增 | ... |
| 子项目 1 API | 后端服务 | 8050 | `api-项目1.域名` |
| 子项目 2 API | 后端服务 | 8051 | `api-项目2.域名` |

> 新增子项目时按此规则分配端口，并更新本表。

## 子项目列表

| 子项目 | 技术栈 | 状态 | 说明 |
|--------|--------|------|------|
| Portfolio | Astro 6 | active | 个人主页主站 |
