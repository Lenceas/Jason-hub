# RELEASE.md — 发布工作流

> Jason-hub 标准化提交流程，配合 Claude Code 自动化执行。
> 在对话中直接说 **"提交代码"** 即可触发完整流程。

---

## 流程全貌

```
开发完成 → 说"提交代码"
              │
              ▼
    ┌─────────────────────┐
    │ ① 预检               │ ← 分支、同步、构建验证
    └────────┬────────────┘
              │
              ▼
    ┌─────────────────────┐
    │ ② 代码审查           │ ← git diff + code-review skill
    └────────┬────────────┘
              │
              ▼
    ┌─────────────────────┐
    │ ③ 分类变更类型       │ ← 确定 commit 前缀 + 版本位
    └────────┬────────────┘
              │
              ▼
    ┌─────────────────────┐
    │ ④ 更新 MD 文档       │ ← 按变更范围更新对应文档
    └────────┬────────────┘
              │
              ▼
    ┌─────────────────────┐
    │ ⑤ 生成改动摘要       │ ← 变更列表 + 版本号 → 输出供审查
    └────────┬────────────┘
              │  (用户确认)
              ▼
    ┌─────────────────────┐
    │ ⑥ 自动执行           │ ← 改版本 → commit → tag → push
    └─────────────────────┘
```

---

## 详细步骤

### ① 预检

- **确认分支**：当前在 `main` 上，如不在则提示
- **同步远程**：`git pull`，确保无冲突
- **环境状态**：无未跟踪的临时文件（`.env`、`node_modules` 等）
- **构建验证**：`cd Portfolio && npm run build` 必须通过，否则中止流程

> 预检仅做检查，不修改任何文件。

### ② 代码审查

- 执行 `git diff` 审查当前改动
- 使用 code-review skill 检查正确性、复用性、效率
- 发现问题直接修复到工作区

### ③ 变更分类

根据改动内容确定 **commit 类型前缀** 和 **版本位**：

| 变更内容 | Commit 前缀 | 版本位 | npm 命令 |
|---------|------------|--------|----------|
| 新功能、新组件 | `feat` | 次版本 | `npm version minor` |
| Bug 修复 | `fix` | 修订 | `npm version patch` |
| 文档更新（README/CHANGELOG 等） | `docs` | 修订 | `npm version patch` |
| 样式调整（CSS/动画/布局） | `style` | 修订 | `npm version patch` |
| 重构（不改变外部行为） | `refactor` | 修订 | `npm version patch` |
| 配置/依赖/CI 变更 | `chore` | 修订 | `npm version patch` |
| 新增子项目 | `feat` | 次版本 | `npm version minor` |

> 默认小改动使用 `patch`，如发现有新功能特征则自动升级为 `minor`。

### ④ 更新 MD 文档

维护规则，按变更范围逐项检查：

| 文档 | 更新时机 | 说明 |
|------|---------|------|
| `CHANGELOG.md`（根目录） | **每次必更** | Monorepo 级别更新日志 |
| `{子项目}/Portfolio-Changelog.md` | **子项目改动必更** | 子项目级更新日志 |
| `README.md`（根目录） | **每次必更** | 中英文双端结构图 + 快速开始必须同步 |
| `{子项目}/README.md` | **子项目改动必更** | 组件列表 + 目录结构 + 配置同步 |
| `DEPLOY.md` | 部署相关改动必更 | 端口/域名/docker-compose/Nginx |
| `ARCHITECTURE.md` | 架构/组件图变动时 | 架构文档 |
| `AGENTS.md` | 技术栈/目录结构/端口变动时 | AI 协作说明 |
| `STYLE_GUIDE.md` | 编码规范变动时 | 命名规范/Git 规范 |

### ⑤ 改动摘要

输出结构：

```
文件变更清单
  ├── 新增：xxx
  ├── 修改：xxx
  └── 删除：xxx

Commit 信息预览
  └── {type}: {简短描述} v{版本}

版本号变化
  └── vX.X.X → vX.X.Y
```

等待用户确认后继续。

### ⑥ 自动执行

```bash
cd Portfolio
npm version <patch|minor> --no-git-tag-version

cd ..
git add -A
git commit -m "{type}: {描述} v{版本}"

git tag v{版本}
git push && git push --tags
```

---

## 新增子项目补充检查项

当新增一个**完整的子项目**时（非小改动），额外检查：

| 检查项 | 文件 | 说明 |
|--------|------|------|
| 添加项目卡片 | `Portfolio/src/data/projects.json` | 新增一条记录 |
| 更新端口表 | `AGENTS.md` | 分配前端端口 + API 端口 |
| 更新架构图 | `ARCHITECTURE.md` | 子项目体系章节 |
| 添加 docker-compose service | `DEPLOY.md` | 容器编排 + Dockerfile |
| 添加 Nginx 配置 | `DEPLOY.md` | 域名 + 反代规则 |
| 更新根 README 结构 | `README.md` | 目录结构图 |
| 建子项目 README | `{子项目}/README.md` | 子项目技术栈/结构/开发说明 |

---

## 快速参考

```
说"提交代码"
  → ① 预检（自动构建验证）| ② 审查 | ③ 分类 | ④ 更新文档 | ⑤ 输出摘要
  → 你确认
  → ⑥ 自动提交 + 标签 + 推送
```
