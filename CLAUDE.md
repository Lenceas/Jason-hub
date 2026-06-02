# Jason-hub Monorepo

> 本文件由 Claude Code 自动维护，每次开发阶段完成或发布版本时更新。
> 换电脑后 `git pull`，新会话自动加载最新进度。

---

> ⚠️ **敏感信息红线**：无论代码还是文档，不得出现明文密码、Token、API Key、连接字符串含密码、密钥等敏感信息，一律使用环境变量或 `***` 占位。发布流 2c 阶段自动检测，发现即拦截。

## 项目速览

| 项目 | 值 |
|------|-----|
| 在线地址 | https://lujiesheng.cn |
| 服务器 | 81.71.136.3（腾讯云 2C4G / Ubuntu 24.04） |
| 当前版本 | v1.3.0 |
| 技术栈 | Astro 6 / Vue 3 + TypeScript / .NET 10 |
| 数据库 | MySQL 8.4 / Redis 8 / MongoDB 8 |
| CI/CD | GitHub Actions → SCP → docker compose up |

---

## 当前工作状态

> `[进行中]` 表示正在做的，`[待办]` 表示计划内的，`[完成]` 表示已交付的。

### 本期焦点：基础设施服务 + Monitor 监控面板

- [完成] Auth 鉴权服务开发 — JWT + RS256 本地验证，端口 8100 ✅
  - .NET 10 + Minimal API + SqlSugar + BCrypt
  - 6 大 API 端点 + Scalar API 文档 + 登录页 HTML
  - AuthShared 共享中间件库供各子项目引用
  - 数据库 `jason_auth`（auth_users / auth_clients / auth_refresh_tokens）
- [完成] Auth 鉴权服务部署 — DNS / SSL / Nginx / docker-compose ✅
- [完成] Auth v0.3.0 — 登录页全面重做 + RSA 前端加密 + 安全增强 + 审计日志 + 体验打磨 ✅
- [完成] Auth v0.3.1 — 修复 Token 通过 URL 参数泄露的安全问题 ✅
- [完成] Auth v0.4.0 — 新增 `/api/v1/auth/me` 用户信息端点 ✅
- [进行中] Notification 通知服务 / 任务调度 / 消息队列 — 方案待定
- [进行中] Monitor 子项目 — 方案确定，待开发
  - 前端：Vue 3 + TypeScript / UnoCSS / ECharts / Pinia（端口 8001）
  - 后端：.NET 10 / SqlSugar / Scalar / Minimal API（端口 8051）
  - 域名：`monitor.lujiesheng.cn` / `api-monitor.lujiesheng.cn`
  - 方案文档：`Monitor/PLAN.md`（6 大模块 + CI/CD 流水线监控）

### 最近完成

- [完成] auth-v0.3.1 — 修复 Token 通过 URL 参数泄露的安全问题
- [完成] auth-v0.3.0 — Auth 登录页全面重做 + RSA 前端加密 + 安全增强 + 审计日志 + 体验打磨
- [完成] v1.3.0 — Auth 鉴权服务部署上线 + 敏感信息规范 + 分支策略修正
- [完成] auth-v0.2.0 — Auth 鉴权服务部署上线（docker-compose / SSL / Nginx / CI/CD）
- [完成] v1.2.0 — Auth 鉴权服务开发 + 数据库 `jason_` 命名规范 + 中文表注释
- [完成] v1.1.0 — 文档体系全面重构（PLAN.md 规范/双轨发布流/中英双语/18 项审计修复）

---

## 关键约定

### 开发规范
- 子项目目录使用 PascalCase（`Monitor/`）
- 根目录和每个子项目各自包含标准 MD 文档：根 `PLAN.md`（总体规划）/ 子项目 `PLAN.md`（方案设计）、`CHANGELOG.md`（日志）、`README.md`（技术说明）
- 样式使用 UnoCSS + Portfolio CSS 变量（监控面板）
- ORM 使用 SqlSugar（支持多数据库/多租户/MongoDB）

### 发布流程
- 子项目动工前必须先出方案文档 → 用户确认
- 提交代码必须走 RELEASE.md 的 7 步流程
- CHANGELOG.md 和 CLAUDE.md 每次必更

### 分支策略
- `main` — 始终可部署，合并即发布
- `project/<name>` — 子项目开发分支
- `feat/<描述>` / `fix/<描述>` — 小功能/修复
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

| 日期 | 决策 | 参考文档 |
|------|------|---------|
| 2026-05-30 | Auth 鉴权服务 v1 开发完成（.NET 10 + RS256 + Scalar） | `Auth/PLAN.md` |
| 2026-05-30 | 数据库命名规范统一为 `jason_<项目名>` 格式 | `PLAN.md` |
| 2026-05-30 | 端口段重定义：三段式（前端 8000-8049 / 子项目 API 8050-8099 / 基础设施服务 8100-8149） | `ARCHITECTURE.md` |
| 2026-05-30 | Auth 鉴权服务纳入基础设施层：JWT + RS256 本地验证（:8100） | `PLAN.md` |
| 2026-05-30 | 通知服务从 Monitor 抽取为独立基础设施服务（Notification :8110） | `PLAN.md` |
| 2026-05-30 | 定时服务与任务调度合并为统一任务调度服务（:8120） | `PLAN.md` |
| 2026-05-30 | 文档体系重构：PLAN.md 规范 + 中英双语 + 根+子项目双层文档 | `STYLE_GUIDE.md` |
| 2026-05-30 | 双轨发布流：子项目独立发布流 + 主仓库发布流 | `RELEASE.md` |
| 2026-05-30 | 子项目独立版本管理：分支内自由发版，main +1 minor | `STYLE_GUIDE.md` |
| 2026-05-30 | Monitor 方案扩展为 6 大模块（+Nginx 监控 + 数据库深度指标 + CI/CD 流水线） | `Monitor/PLAN.md` |
| 2026-05-30 | 采集代理纳入 Monitor 架构体系 | `ARCHITECTURE.md` |
| 2026-06-01 | 主发布流新增步骤⑧：根仓库规范变更自动同步到子项目分支（`--ff-only`） | `RELEASE.md` |
| 2026-06-01 | Auth 登录密码 RSA-OAEP 前端加密（Web Crypto API + JwtService 解密） | `Auth/Program.cs` |
| 2026-06-01 | Auth 安全响应头 + ForwardedHeaders 真实 IP + 审计日志 [IP] [UA] | `Auth/Program.cs` |
| 2026-06-02 | Auth v0.3.1 — 移除登录成功跳转 URL 中的 `?token=`，Token 仅通过 HttpOnly Cookie 传递 | `Auth/Program.cs` |

---

## 加载项目指令

当我说"**加载项目**"时，你需要：

1. 读取本文件（CLAUDE.md）—— 已自动加载
2. 读取根目录全部 `.md` 文档（PLAN.md / ARCHITECTURE.md / DEPLOY.md / RELEASE.md / AGENTS.md / STYLE_GUIDE.md / README.md / CHANGELOG.md）
3. 读取各子项目的 PLAN.md / README.md / CHANGELOG.md
4. 整理项目全景后输出摘要，然后等待我的下一步指令
