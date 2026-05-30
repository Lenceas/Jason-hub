# Jason-hub Monorepo

> 本文件由 Claude Code 自动维护，每次开发阶段完成或发布版本时更新。
> 换电脑后 `git pull`，新会话自动加载最新进度。

---

## 项目速览

| 项目 | 值 |
|------|-----|
| 在线地址 | https://lujiesheng.cn |
| 服务器 | 81.71.136.3（腾讯云 2C4G / Ubuntu 24.04） |
| 当前版本 | v1.1.0 |
| 技术栈 | Astro 6 / Vue 3 + TypeScript / .NET 10 |
| 数据库 | MySQL 8.4 / Redis 8 / MongoDB 8 |
| CI/CD | GitHub Actions → SCP → docker compose up |

---

## 当前工作状态

> `[进行中]` 表示正在做的，`[待办]` 表示计划内的，`[完成]` 表示已交付的。

### 本期焦点：Monitor 监控面板

- [进行中] Monitor 子项目 — 方案确定，待开发
  - 前端：Vue 3 + TypeScript / UnoCSS / ECharts / Pinia（端口 8001）
  - 后端：.NET 10 / SqlSugar / Scalar / Minimal API（端口 8051）
  - 域名：`monitor.lujiesheng.cn` / `api-monitor.lujiesheng.cn`
  - 方案文档：`Monitor/PLAN.md`（6 大模块 + CI/CD 流水线监控）

### 最近完成

- [完成] v1.1.0 — 文档体系全面重构（PLAN.md 规范/双轨发布流/中英双语/18 项审计修复）
- [完成] v1.0.19 — 新增 CLAUDE.md 跨设备记忆同步机制 + 子项目立项流程

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

| 项目 | 前端端口 | API 端口 | 前端域名 | API 域名 |
|------|---------|---------|---------|---------|
| Portfolio | 8000 | — | `lujiesheng.cn` | — |
| Monitor | 8001 | 8051 | `monitor.lujiesheng.cn` | `api-monitor.lujiesheng.cn` |

---

## 关键决策记录

| 日期 | 决策 | 参考文档 |
|------|------|---------|
| 2026-05-30 | 文档体系重构：PLAN.md 规范 + 中英双语 + 根+子项目双层文档 | `STYLE_GUIDE.md` |
| 2026-05-30 | 双轨发布流：子项目独立发布流 + 主仓库发布流 | `RELEASE.md` |
| 2026-05-30 | 子项目独立版本管理：分支内自由发版，main +1 minor | `STYLE_GUIDE.md` |
| 2026-05-30 | 全文档审计修复 18 项：DEPLOY.md Note→Monitor / .NET 10 补全 / .env.example | `DEPLOY.md` |
| 2026-05-30 | Monitor 方案扩展为 6 大模块（+Nginx 监控 + 数据库深度指标 + CI/CD 流水线） | `Monitor/PLAN.md` |
| 2026-05-30 | 采集代理纳入 Monitor 架构体系 | `ARCHITECTURE.md` |
