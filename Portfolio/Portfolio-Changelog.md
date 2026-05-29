# CHANGELOG.md — Portfolio 更新日志

---

## v1.0.14 (2026-05-29)

- **Docker Compose**：新增 MySQL / Redis / MongoDB 基础设施服务

---

## v1.0.13 (2026-05-29)

- **Hero**：优化个人简介文案，去重并精简首行

---

## v1.0.12 (2026-05-29)

- **SEO**：title/description/keywords 优化，加入中文姓名 + 技能关键词
- **Meta**：新增 Open Graph / canonical / Twitter Card 元标签
- **favicon**：新增圆形头像 favicon（64×64，`/images/favicon.png`）

---

## v1.0.11 (2026-05-29)

- 无 Portfolio 直接变更，版本跟随根仓库文档更新

---

## v1.0.10 (2026-05-29 11:25)

- 无 Portfolio 直接变更，版本跟随根仓库文档更新

---

## v1.0.9 (2026-05-29 09:56)

- 无 Portfolio 直接变更，版本跟随根仓库文档更新

---

## v1.0.8 (2026-05-28 ~ 2026-05-29 09:44)

- **Footer**：新增底部备案号 `湘ICP备2021003806号-1`，链接工信部备案查询
- **Docker**：新增 `Dockerfile`（多阶段构建）+ `nginx.conf`（静态文件服务）+ `docker-compose.yml`（容器编排）
- **CI/CD**：GitHub Actions 推送自动部署到腾讯云服务器

---

## v1.0.7 (2026-05-27 23:30 ~ 23:31)

- 无 Portfolio 直接变更，版本跟随根仓库同步

---

## v1.0.6 (2026-05-27 23:26 ~ 23:27)

- **README.md**：同步 BackToTop 组件信息到页面组成 / 目录结构 / 视觉特效（中英文）
- **RELEASE.md**：子项目 README 纳入更新检查清单

---

## v1.0.5 (2026-05-27 23:03 ~ 23:12)

- **STYLE_GUIDE.md**：命名规范重构，按场景分为五种风格
- **日志文件更名**：`Portfolio-Update.md` → `Portfolio-Changelog.md`

---

## v1.0.4 (2026-05-27 23:00 ~ 23:03)

- **Config**：关闭 Astro Dev Toolbar；站点域名改为 `lujiesheng.cn`

---

## v1.0.3 (2026-05-27 15:25 ~ 15:57)

- **Hero**：标语改为 JS 驱动循环打字效果，尾缀 `...`，逐字输出 → 停留 5s → 清空重播，光标持续闪烁
- **BackToTop**：滚动阈值 `>200` → `>0`（立即显示）；箭头跳动 + TOP 标签改为默认展示；TOP 标签从左侧移至圆圈上方居中

---

## v1.0.2 (2026-05-27 00:00 ~ 00:26)

- **BackToTop 组件**：右下角固定定位回到顶部按钮
  - 滚动进度环（SVG 圆环随页面滚动填充）
  - 毛玻璃圆形按钮，保持页面风格一致
  - 箭头图标浮动 / 悬停弹跳动画
  - 悬停时左侧滑出 TOP 标签
  - 点击波纹扩散效果
  - 键盘 Enter / Space 支持
  - 三端响应式适配

---

## v1.0.1 (2026-05-26 19:26 ~ 19:54)

- **Values**：社会主义核心价值观替换为程序员风格文字（简洁/可靠/高效/优雅 · 开源/协作/迭代/极致 · 专注/好奇/创造/分享）
- **Hero**：头像替换为个人微信头像
- **tsconfig**：添加 `ignoreDeprecations: "6.0"` 消除 TS baseUrl 弃用警告
- **STYLE_GUIDE.md**：新增命名规范
- 创建本日志文件 `Portfolio-Changelog.md`

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
