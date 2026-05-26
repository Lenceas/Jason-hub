# UPDATE.md — Portfolio 更新日志

---

## v1.0.1 (2026-05-26 19:26 ~ 19:54)

- **Values**：社会主义核心价值观替换为程序员风格文字（简洁/可靠/高效/优雅 · 开源/协作/迭代/极致 · 专注/好奇/创造/分享）
- **Hero**：头像替换为个人微信头像
- **tsconfig**：添加 `ignoreDeprecations: "6.0"` 消除 TS baseUrl 弃用警告
- **STYLE_GUIDE.md**：新增 UPDATE.md 命名规范
- 创建本日志文件 `Portfolio-Update.md`

---

## v1.0.0 (2026-05-24 16:05 ~ 19:28)

### 初始搭建

- 初始化 Astro 6 项目，Astro ^6.3.7
- 配置开发端口 8000，支持局域网访问（--host）
- 配置 TypeScript strict 模式

### 组件开发

| 组件 | 功能 |
|------|------|
| `BaseLayout.astro` | 布局模板，含全局字体(Inter)、滚动动画、鼠标光晕、碰撞弹球三端内联 JS |
| `Header.astro` | Logo + 个人主页副标题 |
| `Hero.astro` | 头像、打字机动画标语、个人简介、社交链接(GitHub/Gitee/博客园/Email) |
| `Skills.astro` | 9 项技能标签云，按熟练度分色，带图标 |
| `Values.astro` | 价值观展示，12 词自动轮播高亮 |
| `Divider.astro` | 水墨风格装饰分隔线 |
| `ProjectGrid.astro` | 项目卡片网格容器 |
| `ProjectCard.astro` | 毛玻璃卡片，含缩略图/状态标签/标签云/精选标识 |
| `Footer.astro` | 版权 + 构建说明 |

### 数据与样式

- `projects.json` 录入 4 个项目卡片数据
- `global.css` 蓝白配色 CSS 自定义属性、Reset、响应式断点、AOS 动画、玻璃态通用 class
- Mobile First 响应式：≥640px(平板) / ≥1024px(桌面)
- SVG 资源：avatar.svg / logo.svg / placeholder-project.svg
