---
name: release
description: 主仓库发布流程 — main 分支正式发布上线（7 步 + 子项目分支同步）
---

# 发布流程（主仓库）

在对话中说 **"提交代码"** 或调用 `/release` 即可触发。

> 此流程在 `main` 分支上执行，用于正式发布上线。
> **子项目开发期间请走 `/project-release`**，开发完成后再通过子项目流程步骤⑧合并到 main 并走本流程。

## 流程

```
开发完成 → 说"提交代码" / /release
               │
               ▼
    ┌──────────────────────────┐
    │ ① 预检                   │ ← 分支、同步、构建验证
    │   └ 可调用 /verify        │ ← 自动运行构建 + 验证产物
    └────────┬─────────────────┘
              │
              ▼
    ┌──────────────────────────┐
    │ ② 代码审查               │ ← git diff + 多维度审查
    │   ├ /code-review          │ ← 查 bug + 复用性 + 效率（必须）
    │   └ /simplify             │ ← 重构优化（可选，改造成本高时用）
    └────────┬─────────────────┘
              │
              ▼
    ┌──────────────────────────┐
    ┌──────────────────────────┐
    │ ③ 镜像构建判断           │ ← 问用户：本次是否需构建 Docker 镜像？
    │   └ 是 → docker build + push TCR
    │   └ 否（仅文档/规范变更）→ 跳过
    └────────┬─────────────────┘
              │
              ▼
    ┌──────────────────────────┐
    │ ④ 识别变更范围            │ ← 确定 commit 前缀 + 版本位 + 影响文档
    └────────┬─────────────────┘
              │
              ▼
    ┌──────────────────────────┐
    │ ④ 逐项更新 MD 文档        │ ← 必须按清单逐条更新，不得遗漏
    └────────┬─────────────────┘
              │
              ▼
    ┌──────────────────────────┐
    │ ⑤ 文档更新复核            │ ← 向用户公示每份文档检查结果
    └────────┬─────────────────┘
              │
              ▼
    ┌──────────────────────────┐
    │ ⑥ 生成改动摘要            │ ← 变更清单 + 版本号，等待确认
    └────────┬─────────────────┘
              │  (用户确认)
              ▼
    ┌──────────────────────────┐
    │ ⑦ 自动执行                │ ← 改版本 → commit → tag → push
    └──────────────────────────┘

    （可选）同步子项目分支 ← 步骤⑧
```

## 详细步骤

### ① 预检

执行以下检查，任何一项不通过则中止：

- **确认分支**：`git branch --show-current` 必须在 `main` 上
- **同步远程**：`git pull --ff-only` 必须无冲突
- **构建验证**：`cd Portfolio && npm run build` 必须通过
- **状态检查**：无 `.env`、`node_modules` 等错误提交的临时文件
- **子项目标签检查**（如从子项目分支合并而来）：检查 `git tag --list '{子项目}-*'`，确保子项目已有版本标签
- **清理已合并的 feat/fix 分支**：自动删除已合并到 main 的 `feat/` 和 `fix/` 分支（本地 + 远程），避免残留分支堆积
  ```bash
  git branch --merged main | grep -E '^\s+(feat/|fix/)' | sed 's/^[[:space:]]*//' | while read branch; do
    git branch -d "$branch" 2>/dev/null && git push origin --delete "$branch" 2>/dev/null && echo "  ✅ 已删除 $branch" || true
  done
  ```
- **子项目分支合规检查**：检查 main 上是否有子项目代码直接提交而非通过 `project/` 分支合并。发现时自动同步到子项目分支保留记录，然后继续发布
  ```bash
  changes=$(git diff --name-only HEAD~1 | grep -E '^(Auth/|Monitor/|Portfolio/)' | grep -v -E '(CHANGELOG\.md|README\.md|PLAN\.md)$' || true)
  if [ -n "$changes" ] && ! git log -1 --format=%s | grep -qi 'merge'; then
    dir=$(echo "$changes" | head -1 | cut -d/ -f1)
    branch="project/$(echo "$dir" | tr '[:upper:]' '[:lower:]')"
    commit=$(git rev-parse HEAD)
    echo "↻  检测到子项目代码直接提交到 main，同步到 $branch ..."
    git checkout "$branch" && git cherry-pick "$commit" && git push origin "$branch"
    git checkout main
    echo "  ✅ 已同步到 $branch，继续发布（下次子项目发布时合并回 main 会跳过已存在的 commit）"
  fi
  ```

> 可选调用 `/verify` 自动执行构建并验证产物正确性。

### ② 代码审查

对当前改动做多维度检查：

| 步骤 | Skill | 说明 | 必选？ |
|------|-------|------|--------|
| 2a | `/code-review` | 检查正确性、复用性、效率 — 发现 bug 直接修复 | **必须** |
| 2b | `/simplify` | 重构优化 — 代码瘦身、消除重复、提升可读性 | 可选（改动量大时推荐） |
| 2c | **敏感信息检测** | `git diff HEAD` + 手动审查 — 扫描密码/Token/密钥/连接串/API Secret 等明文 | **必须** |

> **2c 检测规则**：逐行审查 git diff，不得出现明文密码、Token、API Key、连接字符串含密码、密钥等敏感信息。发现后立即移除，改为 `***` 或环境变量引用。
> 先跑 code-review 确保正确性，再视情况跑 simplify 做质量提升。三项的结果都直接应用到工作区。

### ③ 镜像构建判断

**必须向用户确认：本次变更是否需要构建 Docker 镜像？**

| 变更类型 | 构建镜像？ |
|---------|-----------|
| 代码改动（.cs / .vue / .ts） | ✅ 是 |
| 仅文档/规范（.md） | ❌ 否 |
| docker-compose.yml | ✅ 是 |

> 需要时：`docker build -t ccr.ccs.tencentyun.com/jason-hub/<项目>:latest . && docker push`

### ④ 识别变更范围

**3a. 确定 commit 前缀和版本位：**

| 变更内容 | Commit 前缀 | 版本位 |
|---------|------------|--------|
| 新功能、新组件 | `feat` | `minor` |
| Bug 修复 | `fix` | `patch` |
| 文档更新 | `docs` | `patch` |
| 样式调整（CSS/动画/布局） | `style` | `patch` |
| 重构（不改变外部行为） | `refactor` | `patch` |
| 配置/依赖/CI 变更 | `chore` | `patch` |
| 新增子项目 | `feat` | `minor` |
| 多种类型混合 | 取最高优先级前缀 | 取最高版本位 |

> 步骤③确定的 **`{type}`** 和 **`{versionBump}`**（`patch` 或 `minor`）将直接传递到步骤⑥的摘要和步骤⑦的执行命令，后续不再手动选择。
>
> **子项目版本说明**：子项目分支（如 `project/monitor`）拥有独立版本号（记录在 `{子项目}/CHANGELOG.md`），与 main 版本无关。合并回 main 时，主仓库版本 +1 minor，子项目版本保持独立演进。
>
> **多项目同时变更策略**：同一分支内的变更用一个 commit；main 上同时改多个子项目时，按逻辑单元拆分 commit，各自发布。

**3b. 列出本次影响范围：**

分析 `git diff` 涉及的变更，向用户说明：

1. **本次改动了哪些项目** — Portfolio / Monitor / 基础设施 / 公共文档
2. **是否属于同一逻辑变更** — 是 → 一个 commit 一次性发布；否 → 建议分多次发布，每次走完整 7 步流程
3. **影响哪些文档**（参见步骤④的清单）

### ④ 逐项更新 MD 文档

**强制规则：必须按下方清单逐条检查，每检查完一项向用户反馈状态。**

> ⚠️ **版本号陷阱**：CLAUDE.md 的"当前版本"字段应设为 **发布目标版本**（步骤③确定的旧版本 + versionBump），而非当前实际版本。这样在步骤⑦提交时它被一并推入仓库，无需发布后再次修改。

| # | 文档 | 更新条件 | 本次？ | 检查内容 |
|---|------|---------|--------|---------|
| 1 | `CHANGELOG.md`（根） | **每次必更** | 必更 | 新增版本条目，按时间倒序，列出全部变更点 |
| 2 | **`CLAUDE.md`（根）** | **每次必更** | 必更 | 更新当前工作状态、进度标记、**「当前版本」设为发布目标版本**、关键决策记录 |
| 3 | `PLAN.md`（根） | 子项目生态/路线图/架构变动 | 检测 | 项目全景、子项目关系、技术栈、路线图 |
| 4 | `{子项目}/CHANGELOG.md` | 子项目有改动 | 检测 | 子项目级版本日志同步 |
| 5 | `README.md`（根） | 新增子项目/结构/域名/在线地址变动 | 检测 | 目录结构图、子项目表、在线地址、快速开始 |
| 6 | `{子项目}/README.md` | 子项目组件/技术栈/配置变动 | 检测 | 组件列表、目录结构、技术栈表、开发命令 |
| 7 | `{子项目}/PLAN.md` | 方案设计变动 | 检测 | 与技术实现不一致时更新 |
| 8 | `DEPLOY.md` | 部署流程/端口/域名/Nginx/SSL/Docker 变动 | 检测 | 全文档审阅，与实际配置不一致则更新 |
| 9 | `ARCHITECTURE.md` | 架构层次/部署拓扑/技术选型变动 | 检测 | 架构图、分层描述、技术选型表 |
| 10 | `AGENTS.md` | 技术栈/目录结构/端口分配/域名规则变动 | 检测 | 端口分配表、技术栈表、目录结构 |
| 11 | `STYLE_GUIDE.md` | 命名规范/分支策略/提交格式变动 | 检测 | 全文审阅 |
| 12 | `RELEASE.md` | 发布流程本身有变动 | 检测 | 本文件 |

> **执行方式**：逐项读文件 → 判断是否需要更新 → 如需要则立即编辑 → 完成后打 ✅ 标记。
> **敏感信息红线**：所有受检文档中不得出现明文密码、Token、连接串含密码、API Secret 等。编辑时发现需立即移除（改为 `***` 或环境变量引用）。

### ⑤ 文档更新复核

**必须在进入步骤⑥之前执行。** 向用户输出以下格式的复核报告：

```
📋 文档更新复核

CHANGELOG.md（根）       ✅ 已更新 / ⏭ 无需更新
CLAUDE.md（根）          ✅ 已更新 / ⏭ 无需更新
PLAN.md（根）            ✅ 已更新 / ⏭ 无需更新
{子项目}/CHANGELOG.md   ✅ 已更新 / ⏭ 无需更新
README.md（根）          ✅ 已更新 / ⏭ 无需更新
{子项目}/README.md      ✅ 已更新 / ⏭ 无需更新
{子项目}/PLAN.md        ✅ 已更新 / ⏭ 无需更新
DEPLOY.md               ✅ 已更新 / ⏭ 无需更新
ARCHITECTURE.md         ✅ 已更新 / ⏭ 无需更新
AGENTS.md               ✅ 已更新 / ⏭ 无需更新
STYLE_GUIDE.md          ✅ 已更新 / ⏭ 无需更新
RELEASE.md              ✅ 已更新 / ⏭ 无需更新
```

**如果任何必更项显示 ⏭，必须在进入步骤⑥之前修复。**

### ⑥ 改动摘要

输出格式：

```
文件变更清单
  ├── 新增：xxx
  ├── 修改：xxx
  └── 删除：xxx

涉及项目
  └── Portfolio / Monitor / 基础设施 / 文档（标记本次影响的项目）

Commit 信息预览
  └── {type}: {简短描述} v{新版本}

版本号变化
  ├── 主仓库：v旧版本 → v新版本（{versionBump}）
  └── 子项目：{子项目} v旧 → v新（如有涉及）
```

等待用户回复"ok"或"确认"后继续。

> 版本位 `{versionBump}` 来自步骤③的判定结果，用户确认即表示认可该版本位。

### ⑦ 自动执行

版本位直接使用步骤③确定的 `{versionBump}`，不再手动选择。

```bash
# 1. 更新主仓库版本号（先改版本，再统一暂存）
cd Portfolio && npm version {versionBump} --no-git-tag-version
cd ..

# 2. 暂存所有变更（根据步骤③的影响范围选择对应路径）
#    文档变更：git add *.md .claude/ 等
#    CI/CD：git add .github/ 等
git add <本次变更涉及的路径>
git add Portfolio/package.json Portfolio/package-lock.json

# 3. 如果涉及子项目，同步更新子项目版本号（可选）
# 例如: Monitor/web/package.json 或 Monitor/api/*.csproj 的版本号

# 4. 提交
git commit -m "{type}: {描述} v{新版本}"

git tag v{新版本}
git push && git push --tags
```

> 暂存提示：步骤③ 3b 已确定本次影响范围，`git add` 只添加属于该范围的路径。若本次同时有多个独立逻辑的未暂存改动，说明它们应分开发布，不要用 `git add -A` 打包。

### ⑧ 同步子项目分支（可选）

如果本次变更是**根仓库文档/规范类变更**（CLAUDE.md / RELEASE.md / STYLE_GUIDE.md / AGENTS.md / PLAN.md / DEPLOY.md），自动合并到活跃子项目分支保持同步：

```bash
# 查找所有远程 project/ 分支
git branch -r | grep 'origin/project/' | sed 's|origin/||' | while read branch; do
  git checkout "$branch" && git merge main --ff-only && git push || echo "⚠️  $branch 无法 fast-forward，跳过"
done
git checkout main
```

> ⚠️ 使用 `--ff-only`，非 fast-forward 时跳过并提示手动处理，不阻塞主发布流。
> 仅同步能从 main 快进的子项目分支，含独立改动的分支不受影响。
> CLAUDE.md 的版本号会随同步更新到子项目分支，这是预期行为（CLAUDE.md 版本始终为主仓库版本）。

## 新增子项目补充检查项

新增子项目时，步骤④自动追加以下检查：

| # | 检查项 | 文件 | 说明 |
|---|--------|------|------|
| A | 添加项目卡片 | `Portfolio/src/data/projects.json` | 新增一条记录 |
| B | 更新端口分配表 | `AGENTS.md` | 分配前端端口 + API 端口 |
| C | 更新部署架构图 | `ARCHITECTURE.md` | 子项目体系 + 部署拓扑 |
| D | 追加 docker-compose service | `DEPLOY.md` + `docker-compose.yml` | 容器编排 + Dockerfile（端口绑 `127.0.0.1`） |
| E | 添加 DNS 解析 | 腾讯云 DNS | `<name>.lujiesheng.cn` + `api-<name>.lujiesheng.cn` A → `81.71.136.3` |
| F | 申请 SSL 证书 | 服务器 acme.sh | 独立子域名证书（一个域名一张） |
| G | 追加 Nginx server 块 | `DEPLOY.md` + `/etc/nginx/sites-available/` | 子域名反代 → `127.0.0.1:<port>` |
| H | 更新 CI/CD deploy.yml | `.github/workflows/deploy.yml` | 追加新 service 构建命令 |
| I | 更新根 README 结构 + 子项目表 | `README.md` | 目录结构图 + 子项目表 |
| J | 建子项目 README | `{子项目}/README.md` | 子项目技术栈/结构/开发说明 |
| K | 初始化子项目版本 | `{子项目}/CHANGELOG.md` | 从 `v0.1.0` 开始，独立于主仓库版本 |
| L | 建子项目方案文档 | `{子项目}/PLAN.md` | 与 `README.md` 同步创建 |

## 禁止事项

- ❌ 跳过步骤④直接进入摘要
- ❌ 改为"可以下个版本再更"（必须本版本更新）
- ❌ 用户说"ok"时跳过复核直接提交
- ❌ 遗漏 CHANGELOG.md 更新
- ❌ 摘要阶段才发现有未更新的文档
- ❌ 在代码或文档中明文写入密码、Token、API Key、连接字符串含密码、密钥等敏感信息
