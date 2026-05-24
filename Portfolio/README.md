# Portfolio

> Jason-hub 主站 · 个人主页

基于 **Astro 6** 构建的极简个人主页，聚合展示个人介绍与项目卡片，作为 Jason-hub 所有子项目的统一入口。

## 技术栈

|  |  |
|------|------|
| 框架 | Astro 6 |
| 样式 | 原生 CSS + CSS 自定义属性 |
| 设计 | 极简清爽 · 蓝白配色 |
| 响应式 | Mobile First（三端适配） |

## 页面组成

- **Header** — Logo、导航
- **Hero** — 头像、名称、简介、社交链接
- **ProjectGrid** — 项目卡片网格（数据驱动）
- **Footer** — 版权信息

## 数据源

项目卡片数据存储在 `src/data/projects.json`，添加新子项目时在此文件中追加记录即可。

## 目录结构

```
Portfolio/
├── public/images/          ← 静态资源（头像、Logo、项目封面）
├── src/
│   ├── pages/
│   │   └── index.astro     ← 首页（单页）
│   ├── layouts/
│   │   └── BaseLayout.astro
│   ├── components/
│   │   ├── Header.astro
│   │   ├── Hero.astro
│   │   ├── ProjectCard.astro
│   │   ├── ProjectGrid.astro
│   │   └── Footer.astro
│   ├── data/
│   │   └── projects.json
│   └── styles/
│       └── global.css
├── astro.config.mjs
├── tsconfig.json
└── package.json
```

## 本地开发

```bash
npm install
npm run dev    # → http://localhost:8000
npm run build  # 构建静态文件到 dist/
```
