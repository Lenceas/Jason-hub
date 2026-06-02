---
name: load-project
description: 加载项目全景 — 读取所有文档 + 子项目文档 + 输出摘要，快速进入工作状态
---

# load-project — 加载项目全景观

在对话中说 **"加载项目"** 或调用 `/load-project` 即可触发。

## 流程

### ① 读取根目录文档

读取以下全部 `.md` 文档：

| 文件 | 用途 |
|------|------|
| `PLAN.md` | 总体规划、路线图 |
| `ARCHITECTURE.md` | 架构层次、部署拓扑 |
| `DEPLOY.md` | 部署流程、Nginx、SSL、Docker |
| `RELEASE.md` | 发布流程速查 |
| `AGENTS.md` | 端口分配、技术栈表、目录结构 |
| `STYLE_GUIDE.md` | 编码规范、命名规范 |
| `README.md` | 项目总览、在线地址 |
| `CHANGELOG.md` | 版本变更日志（仅头部最新版本） |

### ② 读取子项目文档

扫描子项目目录（`Auth/`、`Monitor/`、`Portfolio/` 等），读取每个子项目的：

| 文件 | 用途 |
|------|------|
| `{子项目}/PLAN.md` | 方案设计 |
| `{子项目}/README.md` | 技术说明 |
| `{子项目}/CHANGELOG.md` | 改版日志（仅头部最新版本） |

### ③ 环境检查

```bash
git branch --show-current    # 确认当前分支
git status --short           # 查看工作区状态
git log --oneline -5         # 最近 5 条 commit
```

### ④ 输出项目全景摘要

输出结构化的全景摘要：

```
━━━  Jason-hub 项目全景  ━━━

📌 基本信息
  在线地址: https://lujiesheng.cn
  服务器:   81.71.136.3（腾讯云 2C4G / Ubuntu 24.04）
  当前版本: v{version}
  技术栈:   Astro 6 / Vue 3 + TypeScript / .NET 10 + SqlSugar
  数据库:   MySQL 8.4 / Redis 8 / MongoDB 8
  CI/CD:    GitHub Actions → SCP → docker compose up

📍 当前状态
  分支: {branch}
  工作区: {clean / 有未提交变更}
  最近提交: {last 3 commits}

📂 项目结构
  ├── Portfolio/    — 主站（Astro 6 + Vue 3）
  ├── Auth/         — 鉴权服务（.NET 10 + RS256）
  ├── Monitor/      — 监控面板（Vue 3 + .NET 10，待开发）
  ├── templates/    — 脚手架模板
  ├── scripts/      — 工具脚本
  └── .claude/skills/ — 技能命令

⚡ 可用技能
  /release          — 主仓库发布（说"提交代码"）
  /project-release  — 子项目发布（说"发布子项目"）
  /project-init     — 子项目立项（说"规划子项目"）
  /scaffold-dotnet  — 创建 .NET 后端（说"创建 XX 后端"）
  /load-project     — 重新加载项目全景

📋 本期焦点
  {从 CLAUDE.md 提取当前工作状态}

🛠 文档索引
  规范类:  STYLE_GUIDE.md / RELEASE.md / AGENTS.md
  架构类:  ARCHITECTURE.md / DEPLOY.md / PLAN.md
  项目类:  README.md / CHANGELOG.md
```

### ⑤ 等待指令

输出完毕后等待用户的下一步指令。
