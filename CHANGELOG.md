# CHANGELOG.md — Jason-hub 更新日志

---

## v1.0.7 (2026-05-27 23:30 ~ 23:31)

- **README.md**：英文结构图同步补全 CHANGELOG / DEPLOY / RELEASE / README 文档
- **RELEASE.md**：README 维护规则标注"中英文双端同步"

---

## v1.0.6 (2026-05-27 23:26 ~ 23:27)

- **RELEASE.md**：重构发布工作流 — 新增预检（强制构建验证）/ 变更分类 / 新增子项目补充检查表
- **README.md**：根目录结构图补全 CHANGELOG / DEPLOY / RELEASE 文档
- **Portfolio/README.md**：同步 BackToTop 组件到页面组成 / 目录结构 / 视觉特效（中英文）

---

## v1.0.5 (2026-05-27 23:03 ~ 23:12)

- **STYLE_GUIDE.md**：命名规范重构 — 五种风格（UPPER_CASE / PascalCase / kebab-case / camelCase / SCREAMING_SNAKE）按场景分类
- **UPDATE.md → CHANGELOG.md**：统一更名为 CHANGELOG，匹配行业惯例
- **Portfolio-Update.md → Portfolio-Changelog.md**：子项目日志同步更名
- 全量更新所有文档引用（STYLE_GUIDE / RELEASE / 历史记录）

---

## v1.0.4 (2026-05-27 23:00 ~ 23:03)

- **DEPLOY.md**：新增部署规范文档（Docker Compose + Nginx 反向代理 + CI/CD）
- **RELEASE.md**：新增发布工作流文档（标准化提交流程）
- **Portfolio**：关闭 Astro Dev Toolbar；站点域名改为 `lujiesheng.cn`

---

## v1.0.3 (2026-05-27 15:25 ~ 15:57)

- **Hero 组件**：标语改为 JS 驱动循环打字效果（逐字输出 → 停留 5s → 清空重播），保留闪烁光标
- **BackToTop 组件**：滚动即显示 + 箭头跳动 + TOP 标签移至圆圈上方并默认展示

---

## v1.0.2 (2026-05-27 00:00 ~ 00:26)

- **BackToTop 组件**：页面右下角新增「回到顶部」按钮，含滚动进度环、毛玻璃质感、箭头浮动动画、悬停 TOP 标签、点击波纹效果

---

## v1.0.1 (2026-05-26 19:26 ~ 19:54)

- 价值观模块：将社会主义核心价值观替换为程序员风格文字（简洁/可靠/高效/优雅 · 开源/协作/迭代/极致 · 专注/好奇/创造/分享）
- 头像：替换为个人微信头像
- tsconfig：添加 `ignoreDeprecations: "6.0"` 消除 baseUrl 弃用警告
- STYLE_GUIDE.md：新增命名规范（`CHANGELOG.md` 根目录 / `{项目名}-Changelog.md` 子项目）
- 创建根目录 `CHANGELOG.md` 更新日志
- 创建 `Portfolio/Portfolio-Changelog.md` 子项目更新日志

---

## v1.0.0 (2026-05-24 16:05 ~ 19:28)

### feat: 初始化项目结构

- 初始化 Jason-hub Monorepo 项目结构
- 搭建 Portfolio 主站（Astro 6 + 原生 CSS）
- 实现基础组件：Header / Hero / ProjectCard / ProjectGrid / Footer
- 配置项目端口规则（前端 8000–8049，后端 API 8050–8099）

### feat: 组件功能增强

- **Hero 组件**：鼠标跟随光晕、打字机动画标语、装饰渐变圆
- **Skills 组件**：技能标签云，按熟练度分色展示（expert/advanced/intermediate/beginner）
- **Values 组件**：社会主义核心价值观展示，自动轮播高亮动画（鼠标悬停暂停）
- **Divider 组件**：水墨风格装饰分隔线
- **ProjectCard 组件**：毛玻璃效果卡片、精选标识、状态标签

### feat: 页面特效

- 6 色半透明碰撞弹球特效（全页面）
- 滚动淡入上浮动画（IntersectionObserver）
- 全局蓝白配色 + CSS 自定义属性主题系统
- Mobile First 响应式布局（手机/平板/桌面三端适配）

### docs: 文档完善

- README 中英文双语
- ARCHITECTURE.md 架构设计文档
- AGENTS.md AI 协作说明
- STYLE_GUIDE.md 编码规范（命名/Git/版本/提交信息）

### chore: 其他

- 添加 MIT 开源协议
- 配置 Git 分支策略（main / project-* / feat-* / fix-*）
- 项目数据 `projects.json` 录入 4 个项目：Todo App(已完成)、个人博客(进行中)、IoT Dashboard(规划中)、旧版主页(已归档)
