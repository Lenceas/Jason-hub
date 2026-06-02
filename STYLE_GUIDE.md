# STYLE_GUIDE.md

> 编码规范与风格指南，确保 Jason-hub 及其子项目的代码风格统一。

## 命名规范

| 风格 | 规范 | 适用场景 | 示例 |
|------|------|----------|------|
| **UPPER_CASE** | 全大写 + 扩展名 | 约定俗成的根目录项目文档（按字母序排前） | `README.md`, `CHANGELOG.md`, `PLAN.md`, `LICENSE`, `DEPLOY.md` |
| **PascalCase** | 首字母大写 | 其余根目录文档与说明文件 | `AGENTS.md`, `ARCHITECTURE.md`, `CLAUDE.md`, `RELEASE.md`, `STYLE_GUIDE.md` |
| **PascalCase** | 首字母大写 | Vue/Astro 组件文件、子项目目录 | `ProjectCard.astro`, `Hero.astro`, `Monitor/` |
| **kebab-case** | 小写连字符 | 普通文件、配置文件 | `global.css`, `astro.config.mjs`, `docker-compose.yml` |
| **camelCase** | 小写驼峰 | JSON 字段、JS/TS 变量 | `projectId`, `tagline`, `socialLinks` |
| **SCREAMING_SNAKE** | 大写下划线 | 环境变量、常量 | `TODO_DB_CONNECTION`, `MYSQL_ROOT_PW` |

> 根目录和每个子项目各自包含标准 MD 文档体系：
> - **根目录**：`PLAN.md`（总体规划）+ `CHANGELOG.md`（变更日志）+ `README.md`（项目总览）
> - **子项目**：`PLAN.md`（方案设计）+ `CHANGELOG.md`（变更日志）+ `README.md`（技术说明）

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

## .NET 后端规范

### 目录结构（Auth 示例）

```
Auth/
├── api/                        ← 子项目主应用
│   ├── Program.cs              ← 入口（仅服务注册 + 中间件管道）
│   ├── {子项目}Api.csproj
│   ├── Endpoints/              ← 端点层：API 路由映射
│   │   └── {子项目}Endpoints.cs
│   ├── Models/
│   │   ├── {子项目}Models.cs   ← DTO：请求/响应 record
│   │   └── Entities/           ← 数据库实体
│   │       └── {entity}.cs
│   ├── Pages/                  ← 页面（内嵌 HTML）
│   │   └── {name}Page.cs
│   ├── Services/               ← 业务服务层
│   │   └── {name}Service.cs
│   └── Properties/
├── Shared/                     ← 共享库（供所有子项目引用）
│   └── {name}.cs
└── 标准文档 (CHANGELOG / PLAN / README)
```

### 命名规范

| 类型 | 规则 | 示例 |
|------|------|------|
| 目录 | PascalCase，复数 | `Endpoints/` `Services/` `Models/Entities/` |
| .cs 文件 | PascalCase，前缀子项目名 | `AuthModels.cs` `AuthService.cs` |
| .csproj | `{子项目}{模块}.csproj` | `AuthApi.csproj` `AuthShared.csproj` |
| 命名空间 | `{子项目}{模块}` | `AuthApi.Endpoints` `AuthShared` |
| 请求 DTO | `{动词}Request` | `LoginRequest` `TokenRequest` |
| 响应 DTO | `{名词}Response` | `LoginResponse` `JwtTokenResponse` |
| 实体 | `{子项目}{表名}`（无后缀） | `AuthUser` `AuthRefreshToken` |
| 数据库列 | **snake_case** | `password_hash` `failed_attempts` |

### 代码分层

```
Program.cs                    ← 薄层（只做注册 + 管道）
  └── Endpoints/*Endpoints.cs ← 端点映射 + OpenAPI 描述 (WithTags/WithSummary)
        └── Services/*Service.cs ← 核心业务逻辑
              └── Models/Entities/* ← 数据实体
```

- **Program.cs** 不写业务逻辑
- **Endpoints** 只做参数校验 + 调 Service，不写核心逻辑
- **Services** 做核心业务，不暴露 HTTP 细节
- 端点使用链式调用风格：`.WithTags()` `.WithSummary()` `.WithDescription()` `.Produces<T>()`

### 端点命名

```
/api/v1/{子项目}/{资源}    ← 版本化
/api/v1/auth/login          ← 动词
/api/v1/auth/me             ← 代词
/api/v1/auth/public-key     ← 连字符
```

### OpenAPI 文档

- `builder.Services.AddOpenApi()` 中添加 DocumentTransformer 设置 Title / Description / Version
- Version 建议附带运行时版本：`v1.0.0 (.NET 10.0.8)`
- Scalar 配置统一主题 `ScalarTheme.BluePlanet`

> 创建新的 .NET 后端子项目请使用 `/scaffold-dotnet` 技能，基于 `templates/dotnet-service/` 模板一键生成。
> 生成结构：`api/Program.cs` + `Endpoints/` + `Models/` + `Services/` + `Shared/`

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
  "tags": ["Vue 3", "TypeScript"],
  "year": 2026
}
```

## Git 分支与版本规范

### 分支策略（简化 GitHub Flow）

单人开发，保持简单：

| 分支 | 用途 | 说明 |
|------|------|------|
| `main` | 主分支 | 始终可部署，合并即发布 |
| `project/<name>` | 子项目开发 | 子项目代码必须在此分支开发，再合并到 main，禁止直接提交到 main |
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
# ... 开发 ... (持续在分支上迭代)
git checkout main
git merge project/todo-app          # 开发里程碑合并回 main
git tag v1.1.0                      # 打版本标签
git push origin main --tags
# 子项目分支保留！后续开发继续在 project/todo-app 上
```

### 版本号规范

采用语义化版本（SemVer）：`v主版本.次版本.修订`

| 版本变化 | 场景 | 例子 |
|---------|------|------|
| 主版本 | 主站重构、破坏性变更 | `v2.0.0` |
| 次版本 | 新增子项目、新功能 | `v1.1.0` → 加了 TodoApp |
| 修订 | Bug 修复、小优化 | `v1.1.1` → 修了个 bug |

### 子项目版本管理

每个子项目在独立分支上开发时，拥有自己的独立版本号体系：

| 概念 | 规则 |
|------|------|
| 版本记录位置 | 各子项目目录内的 `CHANGELOG.md` |
| 起始版本 | 从 `v0.1.0` 开始（骨架搭建） |
| 开发期间 | 自由发版：`v0.1.0` → `v0.2.0` → ... → `v1.0.0` |
| 合并回 main | main 版本号 +1 次版本（minor），表示"集成了某子项目" |
| 后续更新 | 子项目版本独立演进，main 版本按实际变更范围决定 |
| Portfolio 版本 | 在 `Portfolio/package.json` 中独立记录，不受子项目版本影响 |

**示例**: Monitor 子项目开发到 v1.0.0 后合并 → main v1.0.19 → v1.1.0

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
