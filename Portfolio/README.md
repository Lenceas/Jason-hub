# Portfolio

> [中文](#zh) · [English](#en)

---

<h2 id="zh">中文</h2>

Jason-hub 主站 · 个人主页

基于 **Astro 6** 构建的极简个人主页，聚合展示个人介绍与项目卡片，作为 Jason-hub 所有子项目的统一入口。

### 技术栈

|  |  |
|------|------|
| 框架 | Astro 6 |
| 样式 | 原生 CSS + CSS 自定义属性 |
| 设计 | 极简清爽 · 蓝白配色 |
| 响应式 | Mobile First（三端适配） |

### 页面组成

- **Header** — Logo、导航
- **Hero** — 头像、名称、标语、简介、社交链接（品牌色图标按钮）
- **Skills** — 彩色技能标签云（按熟练度分色）
- **Values** — 水墨风格社会主义核心价值观展示
- **ProjectGrid** — 项目卡片网格（数据驱动，毛玻璃效果）
- **Footer** — 版权信息

### 视觉特效

- 鼠标跟随光晕、背景渐变装饰圆
- 6 色碰撞弹球
- 滚动淡入动画
- 毛玻璃卡片 + 精选标识

### 数据源

项目卡片数据存储在 `src/data/projects.json`，添加新子项目时在此文件中追加记录即可。

### 目录结构

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
│   │   ├── Skills.astro
│   │   ├── Values.astro
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

### 本地开发

```bash
npm install
npm run dev    # → http://localhost:8000
npm run build  # 构建静态文件到 dist/
```

### Git 分支

Portfolio 作为主站，在 `main` 分支上直接开发。如需较大改动，可切 `feat/` 分支：

```bash
git checkout -b feat/xxx
# 开发完成后
git checkout main
git merge feat/xxx
git branch -d feat/xxx
```

版本号跟随根仓库语义化版本，详见 [STYLE_GUIDE.md](../STYLE_GUIDE.md#git-分支与版本规范)。

---

<h2 id="en">English</h2>

Jason-hub Main Site · Personal Homepage

A minimal personal homepage built with **Astro 6**, aggregating personal profile and project cards as the unified entry point for all Jason-hub sub-projects.

### Tech Stack

|  |  |
|------|------|
| Framework | Astro 6 |
| Styling | Vanilla CSS + CSS Custom Properties |
| Design | Minimal & clean · Blue-white palette |
| Responsive | Mobile-first (3 breakpoints) |

### Page Sections

- **Header** — Logo, navigation
- **Hero** — Avatar, name, tagline, bio, social links (brand-colored icon buttons)
- **Skills** — Color-coded skill tags (by proficiency level)
- **Values** — Ink-wash style socialist core values display
- **ProjectGrid** — Data-driven project card grid (glass morphism)
- **Footer** — Copyright & credits

### Visual Effects

- Mouse-follow glow, gradient decorative circles
- 6-color bouncing balls
- Scroll fade-in animation
- Glass morphism cards with featured badges

### Data Source

Project card data lives in `src/data/projects.json`. Add a new entry whenever a new sub-project is created.

### Directory Structure

```
Portfolio/
├── public/images/          ← Static assets (avatar, logo, thumbnails)
├── src/
│   ├── pages/
│   │   └── index.astro     ← Single page entry
│   ├── layouts/
│   │   └── BaseLayout.astro
│   ├── components/
│   │   ├── Header.astro
│   │   ├── Hero.astro
│   │   ├── Skills.astro
│   │   ├── Values.astro
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

### Local Development

```bash
npm install
npm run dev    # → http://localhost:8000
npm run build  # Build static output to dist/
```

### Git Branching

Portfolio is the main site and is developed directly on `main`. For larger changes, use a `feat/` branch:

```bash
git checkout -b feat/xxx
# After development
git checkout main
git merge feat/xxx
git branch -d feat/xxx
```

Versioning follows the root repo's semantic versioning. See [STYLE_GUIDE.md](../STYLE_GUIDE.md#git-分支与版本规范) for details.
