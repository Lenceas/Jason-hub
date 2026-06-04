# Jason-hub Monorepo

> 本文件由 Claude Code 自动维护，每次开发阶段完成或发布版本时更新。
> 换电脑后 `git pull`，新会话自动加载最新进度。

---

> ⚠️ **敏感信息红线**：代码和文档中不得出现明文密码、Token、API Key、连接串含密码、密钥等，一律使用环境变量或 `***` 占位。违反即为红线，禁止提交。

## 项目速览

| 项目 | 值 |
|------|-----|
| 在线地址 | https://lujiesheng.cn |
| 服务器 | 81.71.136.3（腾讯云 2C4G / Ubuntu 24.04） |
| 当前版本 | v1.5.0 |
| 技术栈 | Astro 6 / Vue 3 + TypeScript / .NET 10 |
| 数据库 | MySQL 8.4 / Redis 8 / MongoDB 8 |
| CI/CD | GitHub Actions → SCP → docker compose up |

## 仓库结构

```
Jason-hub/
├── Portfolio/         ← 主站（Astro 6 + Vue 3）
├── Auth/              ← 鉴权服务（.NET 10 + RS256）
├── Monitor/           ← 监控面板（Vue 3 + .NET 10，待开发）
├── templates/         ← 脚手架模板（dotnet-service）
├── scripts/           ← 工具脚本（scaffold-dotnet.sh 等）
├── .claude/skills/    ← Claude Code 技能命令
├── .github/           ← CI/CD 工作流
└── 根文档             ← PLAN / ARCHITECTURE / DEPLOY / AGENTS / STYLE_GUIDE / README / CHANGELOG / RELEASE / CLAUDE
```

---

## 当前工作状态

> `[进行中]` 表示正在做的，`[待办]` 表示计划内的，`[完成]` 表示已交付的。

### 本期焦点：基础设施服务 + Monitor 监控面板

- [完成] Auth 鉴权服务 v0.5.1 ✅ — README 中英双语 + 端口统一
- [完成] Portfolio 用户认证 改为 子项目自行鉴权策略 — Portfolio 纯公开展示，各子项目 SPA 启动时独立检测登录态 ✅
- [完成] Portfolio 项目卡片更新 — Auth（已上线）替换 Todo App 占位，新增 Monitor 卡片，顶部导航 Skills/Projects ✅
- [完成] .NET 后端脚手架 ✅
- [进行中] Notification 通知服务 / 任务调度 / 消息队列 — 方案待定
- [进行中] Monitor 子项目 — 方案确定，待开发
  - 前端：Vue 3 + TypeScript / UnoCSS / ECharts / Pinia（端口 8001）
  - 后端：.NET 10 / SqlSugar / Scalar / Minimal API（端口 8051）
  - 域名：`monitor.lujiesheng.cn` / `api-monitor.lujiesheng.cn`

---

## 关键约定

### 开发规范
- 子项目目录使用 PascalCase（`Monitor/`）
- 根目录和每个子项目各自包含标准 MD 文档：根 `PLAN.md`（总体规划）/ 子项目 `PLAN.md`（方案设计）、`CHANGELOG.md`（日志）、`README.md`（技术说明）
- 样式使用 UnoCSS + Portfolio CSS 变量（监控面板）
- ORM 使用 SqlSugar（支持多数据库/多租户/MongoDB）

### 发布流程
- 子项目动工前必须先出方案文档 → 用户确认（`/project-init`）
- 提交代码走正式发布流程（`/release` 或说"提交代码"）
- 子项目发布走子项目发布流（`/project-release` 或说"发布子项目"）
- CHANGELOG.md 和 CLAUDE.md 每次必更

### 分支策略
- `main` — 始终可部署，合并即发布
- `project/<name>` — 子项目开发分支，子项目代码（Auth/、Monitor/ 等）必须在此分支开发，再合并到 main
- `feat/<描述>` / `fix/<描述>` — 小功能/修复，完成后删除
- 语义化版本：`v主版本.次版本.修订`

### 端口分配

| 项目 | 端口 | 域名 |
|------|------|------|
| **前端页面 (8000-8049)** | | |
| Portfolio | 8000 | `lujiesheng.cn` |
| Monitor 前端 | 8001 | `monitor.lujiesheng.cn` |
| **子项目 API (8050-8099)** | | |
| Monitor API | 8051 | `api-monitor.lujiesheng.cn` |
| **基础设施服务 (8100-8149)** | | |
| Auth 鉴权服务 | 8100 | `api-auth.lujiesheng.cn` |
| Notification 通知 | 8110 | `api-notification.lujiesheng.cn` |
| 任务调度 | 8120 | —（内网） |
| 消息队列 | 8130 | —（内网） |

---

## 关键决策记录

> 早期架构决策（端口段、文档体系、双轨发布流等）已归档到 `ARCHITECTURE.md` 附录。

| 日期 | 决策 | 参考文档 |
|------|------|---------|
| 2026-06-01 | 主发布流新增步骤⑧：根仓库规范变更自动同步到子项目分支（`--ff-only`） | `.claude/skills/release.md` |
| 2026-06-01 | Auth 登录密码 RSA-OAEP 前端加密（Web Crypto API + JwtService 解密） | `Auth/Program.cs` |
| 2026-06-01 | Auth 安全响应头 + ForwardedHeaders 真实 IP + 审计日志 [IP] [UA] | `Auth/Program.cs` |
| 2026-06-02 | Auth v0.3.1 — 移除登录成功跳转 URL 中的 `?token=`，Token 仅通过 HttpOnly Cookie 传递 | `Auth/Program.cs` |
| 2026-06-02 | Auth 代码结构分离：Program.cs 拆为 Endpoints/ + Pages/ + Models/Entities/，端点到 Services 分层 | `Auth/api/` |
| 2026-06-02 | OpenAPI 端点采用链式调用风格 `.WithTags() .WithSummary() .WithDescription() .Produces<T>()` | `AuthEndpoints.cs` |
| 2026-06-02 | IP 城市解析方案：ip2region 国产离线库（11MB），CI/CD 自动下载 + 服务器 cron 每月更新 | `Ip2RegionService.cs` |
| 2026-06-02 | Portfolio 移除 UserAuth — Portfolio 纯公开展示，子项目各自负责鉴权检测 | — |
| 2026-06-02 | .NET 后端脚手架：`templates/dotnet-service/` + `scripts/scaffold-dotnet.sh` | `.claude/skills/scaffold-dotnet.md` |
| 2026-06-02 | CI/CD 仅从 main 触发部署（去重），往 sub-branch 合并不再触发 | `.github/workflows/deploy.yml` |

---

## 快捷技能命令

| 命令 | 说明 | 对应口头指令 |
|------|------|-------------|
| `/release` | 主仓库发布流（7 步） | "提交代码" |
| `/project-release` | 子项目发布流（8 步） | "发布子项目" |
| `/project-init` | 子项目立项流程（4 步） | "规划子项目" |
| `/scaffold-dotnet` | 从模板创建 .NET 后端项目 | "创建 {项目名} 后端" |
| `/load-project` | 加载项目全景，输出结构化摘要 | "加载项目" |

> 说 **"加载项目"** 或调用 `/load-project` 可读取全部文档并输出项目全景摘要。
