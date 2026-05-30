# Jason-hub Monorepo

> 本文件由 Claude Code 自动维护，每次提交代码时更新。
> 换电脑后 `git pull`，新会话自动加载最新进度。

---

## 项目速览

| 项目 | 值 |
|------|-----|
| 在线地址 | https://lujiesheng.cn |
| 服务器 | 81.71.136.3（腾讯云 2C4G / Ubuntu 24.04） |
| 当前版本 | v1.0.19 |
| 技术栈 | Astro 6 / Vue 3 + TypeScript / .NET 10 |
| 数据库 | MySQL 8.4 / Redis 8 / MongoDB 8 |
| CI/CD | GitHub Actions → SCP → docker compose up |

---

## 当前工作状态

> `[进行中]` 表示正在做的，`[待办]` 表示计划内的，`[完成]` 表示已交付的。

### 本期焦点：Monitor 监控面板

- [进行中] Monitor 子项目 — 方案已定，待开发
  - 前端：Vue 3 + TypeScript / UnoCSS / ECharts / Pinia（端口 8001）
  - 后端：.NET 10 / SqlSugar / Scalar / Minimal API（端口 8051）
  - 域名：`monitor.lujiesheng.cn` / `api-monitor.lujiesheng.cn`
  - 方案文档：`Monitor/README.md`

### 最近完成

- [完成] v1.0.19 — 新增 CLAUDE.md 跨设备记忆同步机制 + 子项目立项流程
- [完成] v1.0.18 — 全量审查修复（TS 类型错误/Divider 启用/文档同步）
- [完成] v1.0.17 — 版本位自动串联优化（步骤③→⑥→⑦）
- [完成] v1.0.16 — 发布流整合 /verify + /simplify 技能
- [完成] v1.0.15 — Redis 添加密码认证

---

## 关键约定

### 开发规范
- 子项目目录使用 PascalCase（`Monitor/`）
- 每个子项目根目录必须有 `README.md` 作为方案文档
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
| 2026-05-30 | 新增 CLAUDE.md 跨设备记忆同步机制，每次提交同步更新 | `CLAUDE.md`、`RELEASE.md` |
| 2026-05-30 | Monitor 子项目方案确定 — Vue 3+TS / UnoCSS / .NET 10 / SqlSugar / Scalar | `Monitor/README.md` |
| 2026-05-30 | 发布流整合技能并优化版本位传递 | `RELEASE.md` |
| 2026-05-30 | 严格遵循 MD 文档规范，CHANGELOG 每次必更 | `feedback-strict-md-compliance` |
