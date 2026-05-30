# Portfolio — 方案设计

> 状态：`active` · Jason-hub 第 0 子项目（主站入口）
> 本文档记录 Portfolio 主站的设计决策与架构方案。

---

## 一、项目定位

Portfolio 是 Jason-hub 的**统一入口**，承载个人介绍、技能展示、项目聚合三大职能。用户通过 Portfolio 了解作者并跳转到各子项目。

| 属性 | 说明 |
|------|------|
| 角色 | Monorepo 主站，第 0 子项目 |
| 入口 | `lujiesheng.cn` + `www.lujiesheng.cn` |
| 目标 | 轻量、极速、零 JS 运行时 |
| 受众 | 访客、招聘方、技术同好 |

---

## 二、技术选型

| 模块 | 选型 | 理由 |
|------|------|------|
| 框架 | **Astro 6** | 内容型站点，默认输出静态 HTML，零 JS 运行时，构建速度快 |
| 样式 | **原生 CSS + CSS 自定义属性** | 页面复杂度低，无需 CSS 框架；自定义属性实现全局主题统一管理 |
| 字体 | **Inter**（Google Fonts） | 现代无衬线体，可读性好，中文 Fallback 系统字体 |
| 部署 | **Docker + Nginx** | 容器化多阶段构建，与 Monorepo 其他服务统一编排 |
| 包管理 | **npm** | Astro 生态默认 |

### 为什么不选

| 方案 | 放弃原因 |
|------|---------|
| Vue / React | 主站以内容展示为主，交互简单，SPA 框架过重 |
| Tailwind / UnoCSS | 页面小，原生 CSS 变量足够管理，无需构建时工具链依赖 |
| TypeScript 严格模式以外 | 使用 strict 模式保证代码质量 |

---

## 三、组件架构

### 分类

| 类别 | 组件 | 职责 |
|------|------|------|
| **布局** | `BaseLayout.astro` | SEO meta、TDK、OG 标签、favicon、Inter 字体、全局内联 JS |
| **导航** | `Header.astro` | Logo SVG + 导航文字，底部 border-bottom |
| **个人展示** | `Hero.astro` | 头像、打字机动画标语、简介、社交品牌色按钮、背景光晕 |
| **技能** | `Skills.astro` | 9 项技能标签云，按熟练度分色 + 气泡标签 |
| **价值观** | `Values.astro` | 12 词程序员价值观，三组并排，自动轮播高亮 |
| **装饰** | `Divider.astro` | 水墨风格分隔线（3 段渐变线 + 圆点） |
| **项目** | `ProjectGrid.astro` | 响应式网格容器（1→2→auto-fill 列） |
| **项目** | `ProjectCard.astro` | 毛玻璃卡片、状态标签、精选星标、缩略图、技术标签 |
| **工具** | `BackToTop.astro` | SVG 进度环 + 毛玻璃 + 弹跳箭头 + 键盘支持 |
| **页脚** | `Footer.astro` | 版权、构建说明、备案号 |

### 组件设计原则

- 每个组件职责单一，不做太多事
- 通过 `export interface Props` 声明属性类型
- 部分组件（Skills、Values）使用内联样式 + `is:global`，因 Astro scoped CSS 在动态渲染中存在兼容问题
- 全局 JS（滚动动画、鼠标光晕、碰撞弹球）写在 BaseLayout 的 `<script is:inline>` 中
- 组件内 JS（打字机、BackToTop、轮播）写在各自组件的 `<script>` 中

---

## 四、视觉特效体系

| # | 特效 | 实现方式 |
|---|------|---------|
| 1 | 鼠标跟随光晕 | Hero 区 mousemove → radial-gradient 位置跟踪 |
| 2 | 渐变装饰圆 | 2 个绝对定位径向渐变圆（500px/300px） |
| 3 | 6 色碰撞弹球 | 6 个固定 div，requestAnimationFrame 弹性碰撞 |
| 4 | 打字机循环标语 | 逐字 120ms → 停留 5s → 清空重播，闪烁光标 |
| 5 | 滚动淡入动画 | IntersectionObserver threshold:0.12 |
| 6 | 毛玻璃卡片 | backdrop-filter:blur(12px) + 半透明白底 |
| 7 | 价值观自动轮播 | 1s 间隔循环高亮，悬停暂停 |
| 8 | BackToTop 进度环 | SVG circle stroke-dashoffset 基于滚动百分比 |
| 9 | 弹跳箭头 | @keyframes translateY -5px |
| 10 | 点击波纹 | ::after 伪元素 scale 2.5 → opacity:0 |
| 11 | 卡片 hover | translateY(-4px) + shadow 增强（触摸设备降级） |
| 12 | 精选星标 | 28px 金色圆形 SVG 星标，absolute 定位 |

---

## 五、数据流

```
src/data/projects.json  ──→  index.astro (import)  ──→  排序  ──→  ProjectGrid  ──→  静态 HTML
                                                           │
                                                   featured 优先
                                                   然后按 status:
                                                   completed → active → planning → archived
```

- 数据源为 JSON 文件，构建时读取，输出纯静态 HTML
- 添加新子项目时只需在 `projects.json` 追加一条记录
- 无运行时 API，无数据库依赖

### 项目卡片字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `id` | string | kebab-case 唯一标识 |
| `title` | string | 项目名称 |
| `tagline` | string | 一句话标签 |
| `description` | string | 详细描述 |
| `status` | string | completed / active / planning / archived |
| `featured` | bool | 是否精选星标（可选） |
| `url` | string | 项目链接或部署地址 |
| `thumbnail` | string | 封面图片路径 |
| `tags` | string[] | 技术标签列表 |
| `year` | number | 年份 |

---

## 六、响应式策略

| 断点 | 端 | 布局 |
|------|----|------|
| 默认 | 手机 | 单列，紧凑间距，小字号 |
| `≥ 640px` | 平板 | 双列卡片，适中间距 |
| `≥ 1024px` | 桌面 | auto-fill 自适应多列，完整尺寸 |

采用 Mobile First 原则，默认写手机样式，`@media (min-width: ...)` 向上适配。

---

## 七、设计决策

### 配色体系

```css
--color-primary: #3B82F6        /* 主蓝 */
--color-primary-light: #60A5FA  /* 浅蓝 */
--color-bg: #FFFFFF             /* 背景白 */
--color-bg-alt: #F8FAFC         /* 浅灰底 */
--color-text: #1E293B           /* 深灰文字 */
--color-text-light: #64748B     /* 浅灰文字 */
```

极简蓝白配色，凸显内容，减少视觉干扰。

### 字体

Inter 作为主要英文字体，中文回退到系统字体（`-apple-system, "Microsoft YaHei", sans-serif`）。

### 性能目标

- 零 JS 运行时（特效 JS 仅在首屏加载后执行）
- 构建产物为纯静态文件，由 nginx:alpine 提供服务
- Docker 多阶段构建（node:22 build → nginx:alpine serve）

---

## 八、与子项目关系

```
用户访问 lujiesheng.cn
        │
        ▼
  Portfolio 主站
  （展示个人介绍 + 项目卡片）
        │
        ▼ 点击卡片跳转
        │
  ┌─────┴─────┐
  │            │
Monitor     博客...
（子域名）    （子域名）
```

- Portfolio 不耦合任何子项目的实现细节
- 新增子项目仅需在 `projects.json` 加记录 + 配置 DNS/Nginx
- 各子项目独立开发、独立部署、独立版本

---

> **本文档于 2026-05-30 基于现有代码内容整理，后续架构变更需同步更新。**
