# CHANGELOG.md — Jason-hub 更新日志

---

## v1.3.1 (2026-06-01)

- **docs**：CLAUDE.md 新增"加载项目指令" — 一句话触发项目全景加载

## v1.3.0 (2026-05-31)

- **feat**：Auth 鉴权服务部署上线 — docker-compose 编排 / SSL Let's Encrypt / Nginx 反代
- **feat**：CI/CD 支持 Auth 构建部署（触发 project/auth 分支）
- **security**：Auth `/healthz` 公网 Nginx 拦截（`return 404`）
- **fix**：Auth Dockerfile 构建路径修复
- **docs**：新增敏感信息红线规范（RELEASE.md 2c 检测 / CLAUDE.md 警示）
- **docs**：修复子项目分支策略 — project/ 分支为长期迭代分支，合并后保留
- **fix**：移除文档中的密码明文

## v1.2.0 (2026-05-30)

- **feat**：新增 Auth 鉴权基础设施服务（.NET 10 + Minimal API，:8100）
- **feat**：Auth JWT 签发 — RS256 非对称签名 + BCrypt 密码哈希 + HttpOnly Cookie 跨子域共享
- **feat**：Auth 爆破防御 — IP 限流（5次/分钟）+ 账户锁定（10次/15分钟）
- **feat**：Auth Scalar API 文档集成（BluePlanet 蓝白主题）
- **feat**：AuthShared 共享中间件库 — JwtValidator / AuthHandler / RequireScope
- **feat**：Auth 6 大 API 端点（login / token / refresh / healthz / health / public-key / docs）
- **feat**：Auth 登录页 HTML 蓝白毛玻璃风格
- **feat**：数据库命名规范 — 统一 `jason_` 前缀（jason_auth / jason_monitor / jason_blog / jason_forum）
- **docs**：Monitor/PLAN.md 7 张表 + PLAN.md 3 张表全字段中文注释
- **docs**：PLAN.md 新增数据库命名规范节
- **docs**：Auth 子项目文档三件套（PLAN.md / CHANGELOG.md / README.md）
- **fix**：AuthService 爆破防御并发安全 — 原子 SQL 递增失败计数
- **fix**：Auth JwtService 线程安全 — RS256 签名加锁保护
- **fix**：Auth 刷新令牌流程 — 登录时生成 refresh token + 数据库持久化
- **fix**：Auth RefreshToken 使用 DI 单例 JwtValidator
- **chore**：完善 .gitignore（.NET bin/obj / 开发密钥 / IDE 配置）

## v1.1.0 (2026-05-30)

- **feat**：新增文档体系规范 — 根目录 + 子项目各含 `PLAN.md`（方案）+ `CHANGELOG.md`（日志）+ `README.md`（指南）
- **feat**：新增根 `PLAN.md` 总体规划 + `Portfolio/PLAN.md` 方案设计 + `Monitor/PLAN.md` 扩展为 6 大模块
- **feat**：创建 `.env.example` 环境变量模板，补全缺失的部署前置文件
- **feat**：采集代理（Monitor Agent）纳入架构体系（ARCHITECTURE.md 部署图 + docker-compose）
- **docs**：`Portfolio/Portfolio-Changelog.md` → `Portfolio/CHANGELOG.md`，统一全大写命名
- **docs**：RELEASE.md 新增「子项目发布流程」（8 步，含代码审查/文档复核/合并到 main）
- **docs**：RELEASE.md 主发布流程增强（选择性暂存/子项目版本同步/多项目变更策略）
- **docs**：DEPLOY.md 全篇示例 "Note" → "Monitor"，修复 3 年不一致的占位符问题
- **docs**：AGENTS.md + ARCHITECTURE.md 技术栈表补全 .NET 10 + Scalar
- **docs**：Monitor/README.md 中英双语 + 目录结构标注"规划中"
- **docs**：AGENTS.md 目录结构图补全（从 5 项扩展到 15 项）
- **docs**：CLAUDE.md 更新时机改为"每次开发阶段完成或发布版本时更新"
- **docs**：全文档 18 项审计修复（补全缺失引用/修复不一致/对齐规范）
- **fix**：`projects.json` Todo App 描述 .NET 8 → .NET 10 统一

## v1.0.19 (2026-05-30)

- **新功能**：新增 `CLAUDE.md` 会话记忆同步机制 — 项目状态自动维护，换电脑后 `git pull` 即可续接进度
- **文档**：RELEASE.md 新增「子项目立项流程」先行步骤，文档更新清单增加 CLAUDE.md（每次必更）
- **文档**：ARCHITECTURE.md 子项目体系拆为方案设计 + 开发部署两阶段
- **文档**：AGENTS.md 子项目表新增 Monitor（planning），引用方案文档
- **文档**：STYLE_GUIDE.md 追加子项目 README.md 规范
- **Monitor**：输出完整方案文档至 `Monitor/PLAN.md`（五大模块/技术选型/API/DB/采集方案）

---

## v1.0.18 (2026-05-30)

- **修复**：全量 TypeScript 类型错误修复（BackToTop / Hero / Skills / ProjectCard / index），`astro check` 零错误通过
- **修复**：Hero 打字机文案改为 prop 驱动，消除硬编码
- **修复**：Divider 组件接入页面（Values 与 Skills 之间），原闲置组件正式启用
- **文档**：ARCHITECTURE + Portfolio/README 中 Values 描述同步为"程序员价值观 12 词"
- **文档**：CHANGELOG 数据库版本号修正（Redis 7→8, MongoDB 7→8）
- **文档**：README 子项目命名统一为 PascalCase、STYLE_GUIDE 追加命名规则
- **杂项**：tsconfig 清理、.gitignore 补全、avatar.svg 删除、新增 `npm run check`

---

## v1.0.17 (2026-05-30)

- **RELEASE.md**：版本位自动串联优化 — 步骤③判定的 `{versionBump}` 直接传递到步骤⑥摘要和步骤⑦执行命令，移除手动选择

---

## v1.0.16 (2026-05-30)

- **RELEASE.md**：发布流整合 `/verify`（构建验证）和 `/simplify`（重构优化）技能，代码审查升级为双阶段流程

---

## v1.0.15 (2026-05-29)

- **Redis**：新增密码认证 — `--requirepass ${REDIS_PW}` 启动参数，healthcheck 同步适配带密码验证
- **DEPLOY.md**：同步更新 Redis 配置示例

---

## v1.0.14 (2026-05-29)

- **基础设施**：Docker Compose 新增全局基础设施层 — MySQL 8.4 + Redis 8 + MongoDB 8，所有子项目共用
- **文档**：AGENTS / ARCHITECTURE / DEPLOY 端口表与部署图更新，加入数据库层和 Monitor 子项目

---

## v1.0.13 (2026-05-29)

- **Hero**：优化个人简介文案，去重并精简首行

---

## v1.0.12 (2026-05-29)

- **SEO**：网页 TDK 优化 — title/description/keywords 加入中文姓名 + 技能关键词
- **OG 标签**：新增 Open Graph + Twitter Card 元标签，优化社交媒体分享展示
- **favicon**：新增圆形头像 favicon，替换默认 logo.svg
- **README**：许可证归入各语言段，中/英文内容各自独立

---

## v1.0.11 (2026-05-29)

- **README**：根 README 重构为项目概览（去端口/分支/版本），与子项目 README 风格统一
- **RELEASE**：发布工作流重构 — 强制 7 步流程 + 文档更新复核 + 禁止事项
- **CHANGELOG**：补齐 v1.0.9 / v1.0.10 遗漏记录

---

## v1.0.10 (2026-05-29 11:25)

- **文档**：DEPLOY.md / AGENTS.md / ARCHITECTURE.md / RELEASE.md 全面更新
  - SSL 自动化流程（acme.sh + Let's Encrypt + 腾讯云 DNS API）
  - 域名命名规范（`<name>.lujiesheng.cn` / `api-<name>.lujiesheng.cn`）
  - Nginx 反向代理配置同步实际部署
  - 新增子项目部署检查清单（DNS / SSL / Nginx / CI-CD）
  - 后端框架统一 .NET 10
  - 端口绑定改为 `127.0.0.1` 仅本地回环

---

## v1.0.9 (2026-05-29 09:56)

- **文档**：CHANGELOG.md / DEPLOY.md / Portfolio/CHANGELOG.md 同步实际 CI/CD 实现 + 部署记录

---

## v1.0.8 (2026-05-28 ~ 2026-05-29 09:44)

- **Footer**：新增底部备案号 `湘ICP备2021003806号-1`，链接工信部备案查询
- **Docker**：新增 `Dockerfile` + `nginx.conf` + `docker-compose.yml`，多阶段构建容器化部署
- **CI/CD**：新增 GitHub Actions 自动部署工作流（`sshpass` + `scp` 直传 → `docker compose up --build -d`）
- **Server**：腾讯云 2C4G 服务器部署上线，`81.71.136.3:8000`

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
- **Portfolio-Update.md → CHANGELOG.md**：子项目日志同步更名
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
- STYLE_GUIDE.md：新增命名规范（`CHANGELOG.md` 根目录 / `{子项目}/CHANGELOG.md` 子项目）
- 创建根目录 `CHANGELOG.md` 更新日志
- 创建 `Portfolio/CHANGELOG.md` 子项目更新日志

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
