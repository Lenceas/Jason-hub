# RELEASE.md — 发布工作流

> Jason-hub 的标准化提交流程，配合 Claude Code 自动化执行。
> 在对话中直接说 **"提交代码"** 即可触发完整流程。

---

## 流程概览

```
开发完成 → 说"提交代码"
              │
              ▼
    ① 代码审查（code-review）
              │
              ▼
    ② 更新 MD 文档（如有需要）
              │
              ▼
    ③ 生成改稿摘要 + 版本号 → 输出供用户审查
              │
              ▼  (用户确认)
    ④ npm version patch --no-git-tag-version
       → package.json 版本号 +1
              │
              ▼
    ⑤ git add + commit + tag v1.0.x + push
```

---

## 详细步骤

### ① 代码审查

- 执行 `git diff` 审查当前改动
- 检查正确性、复用性、效率（低/中 effort，高置信度）
- 如需修复直接应用到工作区

### ② 更新 MD 文档

涉及文档及对应维护规则：

| 文档 | 更新时机 | 说明 |
|------|---------|------|
| `CHANGELOG.md`（根目录） | **每次必更** | Monorepo 级别更新日志 |
| `Portfolio/Portfolio-Changelog.md` | Portfolio 相关改动必更 | 子项目级更新日志 |
| `DEPLOY.md` | 部署架构/端口/域名有变动时 | 部署规范 |
| `README.md` | 项目结构/快速开始有变动时 | 项目总览 |
| `ARCHITECTURE.md` | 架构设计有变动时 | 架构文档 |
| `AGENTS.md` | AI 协作说明需要更新时 | AI 协作说明 |
| `STYLE_GUIDE.md` | 编码规范有变动时 | 编码规范 |

### ③ 版本号规则

采用语义化版本（SemVer）：`v主版本.次版本.修订`

| 版本位 | 变更场景 | 例子 |
|--------|---------|------|
| 修订 | 样式调整、Bug 修复、小优化 | `v1.0.2` → `v1.0.3` |
| 次版本 | 新增子项目、新功能模块 | `v1.0.3` → `v1.1.0` |
| 主版本 | 架构重构、破坏性变更 | `v1.1.0` → `v2.0.0` |

当前小改动默认使用 `npm version patch`（修订位 +1）。

### ④ 自动执行（用户确认后）

```bash
cd Portfolio
npm version patch --no-git-tag-version  # 仅改 package.json，不自动 commit/tag

# 并入版本管理的所有改动
git add -A
git commit -m "style: xxx v1.0.x"

# 打标签并推送
git tag v1.0.x
git push && git push --tags
```

---

## 快速参考

```
说"提交代码" → 我自动执行 ①→②→③ → 你确认 → 我执行 ④→⑤
```
