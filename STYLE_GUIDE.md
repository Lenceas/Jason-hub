# STYLE_GUIDE.md

> 编码规范与风格指南，确保 Jason-hub 及其子项目的代码风格统一。

## 命名规范

| 风格 | 规范 | 适用场景 | 示例 |
|------|------|----------|------|
| **UPPER_CASE** | 全大写 + 扩展名 | 根目录项目文档（约定俗成，按字母序排前） | `README.md`, `CHANGELOG.md`, `LICENSE`, `DEPLOY.md` |
| **PascalCase** | 首字母大写 | Vue/Astro 组件文件、子项目目录 | `ProjectCard.astro`, `Hero.astro`, `Monitor/` |
| **kebab-case** | 小写连字符 | 普通文件、配置文件 | `global.css`, `astro.config.mjs`, `docker-compose.yml` |
| **camelCase** | 小写驼峰 | JSON 字段、JS/TS 变量 | `projectId`, `tagline`, `socialLinks` |
| **SCREAMING_SNAKE** | 大写下划线 | 环境变量、常量 | `TODO_DB_CONNECTION`, `MYSQL_ROOT_PW` |

> 例外：子项目级更新日志使用 `{项目名}-Changelog.md`（如 `Portfolio-Changelog.md`）。

## CSS 规范

- 使用 **CSS 自定义属性**管理主题色，变量定义在 `global.css` 的 `:root` 中
- 采用 **Mobile First** 响应式：默认写手机样式，`@media (min-width: ...)` 向上适配
- 标准断点：
  - `≥ 640px` — 平板
  - `≥ 1024px` — 桌面
- 避免 `!important`
- 颜色值统一引用 CSS 变量，不写死
- 部分组件（Skills、Values）使用**内联样式**，因 Astro 的 scoped CSS 在动态渲染中存在兼容问题

## 组件规范

- **Astro 组件**：模板 + `<style>` 写在同一个 `.astro` 文件中
- 组件通过 `export interface Props` 声明属性类型
- 每个组件职责单一，不做太多事
- 部分组件使用内联样式 + `is:global` 处理响应式（避免 scoped CSS 不生效的问题）

## 项目数据规范

`projects.json` 中每条记录的字段：

```json
{
  "id": "kebab-case-id",
  "title": "项目名称",
  "tagline": "一句话标签",
  "description": "详细描述",
  "status": "active | planning | archived",
  "url": "https://项目部署地址",
  "thumbnail": "/images/项目封面.svg",
  "tags": ["Vue3", "TypeScript"],
  "year": 2026
}
```

## Git 分支与版本规范

### 分支策略（简化 GitHub Flow）

单人开发，保持简单：

| 分支 | 用途 | 说明 |
|------|------|------|
| `main` | 主分支 | 始终可部署，合并即发布 |
| `project/<name>` | 子项目开发 | 每个子项目独立分支，开发完合并回 main 后删除 |
| `feat/<描述>` | 功能开发 | 某个子项目内的小功能（可选粒度） |
| `fix/<描述>` | 修复 | Bug 修复 |

### 工作流程

```
main ───●────────●────────●────────  ← 始终可部署
         \      /        /
project/   ●──●──●      /
todo-app        \      /
                 ●────●

Tag: v1.0.0    v1.1.0    v1.1.1
```

**开发一个新子项目：**
```bash
git checkout -b project/todo-app    # 从 main 切出子项目分支
# ... 开发 ...
git checkout main
git merge project/todo-app          # 合并回 main
git branch -d project/todo-app      # 删除开发分支
git tag v1.1.0                      # 打版本标签
git push origin main --tags
```

### 版本号规范

采用语义化版本（SemVer）：`v主版本.次版本.修订`

| 版本变化 | 场景 | 例子 |
|---------|------|------|
| 主版本 | 主站重构、破坏性变更 | `v2.0.0` |
| 次版本 | 新增子项目、新功能 | `v1.1.0` → 加了 TodoApp |
| 修订 | Bug 修复、小优化 | `v1.1.1` → 修了个 bug |

> 版本号代表整个 Monorepo 的里程碑状态。若后续需要单独发布子项目，可拆分为独立仓库或使用子项目级 tag（如 `todoapp-v1.0.0`）。

### 提交信息规范

```
<type>: <简短描述>

可选详细说明
```

**类型前缀：**
| 前缀 | 说明 |
|------|------|
| `feat` | 新功能 |
| `fix` | 修复 |
| `style` | 样式调整 |
| `docs` | 文档更新 |
| `refactor` | 重构 |
| `chore` | 杂项（配置、依赖等） |

**示例：**
```
feat: 添加 ProjectCard 组件

实现项目卡片的基础布局，包含缩略图、标签、状态标识。
```

## 端口规则

详见 [AGENTS.md](./AGENTS.md) 端口分配表。
