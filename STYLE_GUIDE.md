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
>
> **README 语言规范：**
> | 层级 | 语言 | 示例 |
> |------|------|------|
> | 根目录 README.md | **中英双语** | 当前已是双语 |
> | 子项目根 README.md | **中英双语** | `Auth/README.md`、`Monitor/README.md` |
> | 子项目内部 README.md | **中文** | `Monitor/web/README.md`、`Auth/api/README.md`（如存在） |
> | 其他文档（CHANGELOG / PLAN） | **中文** | 全文中文即可 |

## CSS 规范

- 使用 **CSS 自定义属性**管理主题色，变量定义在 `global.css` 的 `:root` 中
- 采用 **Mobile First** 响应式：默认写手机样式，`@media (min-width: ...)` 向上适配（Portfolio 主站）
- 标准断点（子项目前端通用）：
  - `< 480px` — 小屏手机（单列卡片）
  - `≥ 480px` — 手机（≤2 列卡片）
  - `≥ 768px` — 平板
  - `≥ 1024px` — 桌面
- 避免 `!important`

### 子项目前端响应式（Vue 3 必需）

> 所有 Vue 3 子项目前端必须支持三端响应式，不可仅适配桌面。

| 要求 | 说明 |
|------|------|
| **侧边栏** | 手机端隐藏，通过汉堡菜单按钮滑入，平板端可收窄 |
| **卡片网格** | 手机端 ≤2 列，平板/桌面 auto-fill |
| **图表容器** | 手机端单列，平板及以上双列 |
| **数据表格** | 容器加 `overflow-x: auto`，手机端字号缩小，不换列表头 |
| **间距** | 手机端 `gap: 8px` / `padding: 12px`，桌面端 `12-16px` |
| **viewport** | `index.html` 必须含 `<meta name="viewport" content="width=device-width, initial-scale=1.0">` |
| **CSS 变量** | 响应式断点用 CSS 变量控制，如 `--sidebar-width` 在 `@media` 中调整 |

> 示例见 `Monitor/web/src/layouts/DashboardLayout.vue`（汉堡菜单 + overlay + CSS 变量断点）。

### 时序图表（ECharts 强制）

> 所有时序趋势图表必须使用 `xAxis: { type: 'time' }` 而非 `type: 'category'`。

| 规则 | 说明 |
|------|------|
| **xAxis 类型** | 必须 `type: 'time'`，传入 `min/max` 时间戳实现滑动窗口 |
| **数据格式** | `series.data` 为 `[timestamp, value][]` 二元组，不是字符串标签 |
| **滑动窗口** | 通过 `xMin`/`xMax` 响应式变量控制，每 N 秒推进一次 |
| **后端聚合** | 数据量大（>200 点）时后端做时间桶平均，前端直接渲染 |
| **动画** | `animation: false`，靠时间轴平移实现平滑滚动，避免抽搐 |
| **多图表对齐** | 所有图表共用同一个 `xMin/xMax`，横轴天然对齐 |

```ts
// ✅ time axis
xAxis: { type: 'time', min: props.xMin, max: props.xMax }
series: [{ data: [[1760000000, 45], [1760000060, 52]] }]

// ❌ category axis（时序数据不要用）
xAxis: { type: 'category', data: ['13:00', '13:05'] }
series: [{ data: [45, 52] }]
```

> 示例见 `Monitor/web/src/components/LineChart.vue`

### 前端代码质量（强制）

> 所有 Vue 3 子项目必须配置 ESLint + TypeScript 检查，`build` 前自动执行。

| 规则 | 说明 |
|------|------|
| **ESLint** | 必须配 `eslint.config.mjs`，基础规则 = `@eslint/js` + `typescript-eslint` + `vue-eslint-plugin` |
| **TypeScript** | 必须 `vue-tsc --noEmit`，零类型错误 |
| **build 脚本** | `"build": "npm run lint && npm run typecheck && vite build"`，任一失败即中止 |
| **清理命令** | 必须提供 `lint` / `lint:fix` / `typecheck` 三个脚本 |

```json
// package.json
{
  "scripts": {
    "lint": "eslint . --max-warnings 0",
    "lint:fix": "eslint . --fix",
    "typecheck": "vue-tsc --noEmit",
    "build": "npm run lint && npm run typecheck && vite build"
  }
}
```
- 颜色值统一引用 CSS 变量，不写死
- 部分组件（Skills、Values）使用**内联样式**，因 Astro 的 scoped CSS 在动态渲染中存在兼容问题

## 组件规范

- **Astro 组件**：模板 + `<style>` 写在同一个 `.astro` 文件中
- 组件通过 `export interface Props` 声明属性类型
- 每个组件职责单一，不做太多事
- 部分组件使用内联样式 + `is:global` 处理响应式（避免 scoped CSS 不生效的问题）

## .NET 后端规范

### 数据库索引（强制）
> 所有涉及 `WHERE` / `ORDER BY` / `JOIN` 查询的字段必须建索引。两步走：实体类加 `[SugarIndex]` 标记 + Program.cs 加迁移脚本。

| 规则 | 说明 |
|------|------|
| 查询字段必建 | 但凡出现在 `Where()`、`OrderBy()`、`Join()` 中的属性，必须建索引 |
| 复合索引 | 经常一起查询的多字段用联合索引，字段顺序 = 查询频率降序 |
| 声明位置 | 实体类级别 `[SugarIndex]` 用于文档 + 脚手架生成，实际由 IndexMigration 创建 |
| 迁移脚本 | Program.cs CodeFirst 之后用 `CREATE INDEX IF NOT EXISTS` 补齐，不丢历史数据 |

```csharp
// 实体：声明索引意图
[SugarTable("uptime_records")]
[SugarIndex("idx_site_checked", nameof(SiteId), OrderByType.Asc, nameof(CheckedAt), OrderByType.Desc)]
public class MonitorUptimeRecord { ... }

// Program.cs「索引迁移」段（CodeFirst 之后执行）
// SqlSugar CodeFirst 不会补建已有表的索引，需手动迁移
var indexes = new (string table, string name, string cols)[]
{
    ("server_metrics", "idx_ts", "Ts"),
    ("uptime_records", "idx_site_checked", "SiteId, CheckedAt"),
};
foreach (var (table, name, cols) in indexes)
{
    try { db.Ado.ExecuteCommand($"CREATE INDEX IF NOT EXISTS {name} ON {table} ({cols})"); }
    catch { /* 已存在则跳过 */ }
}
```

> 新增查询逻辑 → 同步加 `[SugarIndex]` + 索引迁移条目。生产环境 `IF NOT EXISTS` 保证幂等安全。

### 时间处理（强制）
> 数据库统一存 UTC（`DateTime.UtcNow`），返回给前端的 JSON 统一通过 `DateTimeBjtConverter` 转为北京时间（UTC+8）。

| 层级 | 规则 |
|------|------|
| 数据库写入 | 全部用 `DateTime.UtcNow` |
| JSON 响应 | 全局 `JsonConverter` 自动 `+8h`，无需手动转 |
| 注册方式 | `builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new DateTimeBjtConverter()))` |
| 前端显示 | 直接用 `new Date(isoStr)` 或 `toLocaleString()`，无需额外处理 |

> 示例见 `Monitor/api/Services/DateTimeBjtConverter.cs`（可跨项目复用）。

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

- **Program.cs** 不写业务逻辑。必须包含：
  - `UseForwardedHeaders`（Nginx 反代取真实 IP）
  - 安全响应头中间件（`X-Content-Type-Options` / `X-Frame-Options` / `HSTS` 等）
  - `UseStaticFiles()` — 如有 `wwwroot/` 静态资源（如 Scalar favicon）需要提供
  - `UseCors()`
  - `/api/v1/docs` → 跳转到 Scalar 文档页
- **Endpoints** 只做参数校验 + 调 Service，不写核心逻辑
- **Services** 做核心业务，不暴露 HTTP 细节
- 端点使用链式调用风格，以下方法**全部必填**：

  | 方法 | 说明 | 示例 |
  |------|------|------|
  | `.WithTags("模块名")` | Scalar 分组标签 | `.WithTags("认证")` |
  | `.WithSummary("一句话")` | 端点的简短标题 | `.WithSummary("用户密码登录")` |
  | `.WithDescription("详细")` | 多行说明，含参数/返回值/边界情况 | 参考 Auth 的 `.WithDescription()` |
  | `.Produces<T>(200)` | 成功响应类型 | `.Produces<LoginResponse>(StatusCodes.Status200OK)` |
  | `.Produces(401)` / `.Produces(404)` | 各错误响应（每状态码一行） | `.Produces(StatusCodes.Status401Unauthorized)` |

- 每个端点必须有至少一个 `.Produces<SuccessType>()` + 所有可能错误状态的 `.Produces()`
- `.WithDescription()` 应包含参数说明、边界情况、示例值，多行用 `\n` 分隔

### Program.cs 入口模板

```csharp
// ======== 中间件 ========
app.UseForwardedHeaders(new ForwardedHeadersOptions { ... });
app.Use(async (context, next) => { /* 安全响应头 */ });
app.UseStaticFiles();            // 提供 wwwroot/ 静态资源（Scalar favicon 等）
app.UseCors();

// ======== 端点 ========
app.MapGet("/api/v1/docs", () => Results.Redirect("/scalar/v1"))
   .WithTags("文档").WithSummary("跳转到 Scalar API 文档页面");
app.MapOpenApi();
app.MapScalarApiReference("scalar/v1", options => {
    options.WithTheme(ScalarTheme.BluePlanet)
           .WithFavicon("/images/favicon.png");
});
app.Map{子项目}Endpoints();
```

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

### XML 文档注释规范

所有 .NET 后端代码的 **public 方法**必须加 XML 文档注释，包含以下标签：

| 标签 | 适用场景 | 示例 |
|------|---------|------|
| `<summary>` | 方法用途说明（必填） | `/// <summary>获取最新服务器指标</summary>` |
| `<param name="...">` | 每个入参的说明（必填） | `/// <param name="rangeHours">查询范围（小时），默认24h</param>` |
| `<returns>` | 返回值说明（必填） | `/// <returns>指标列表（按时间正序）</returns>` |
| `<example>` | 可选：用法示例 | `/// <example>var m = await svc.GetAsync();</example>` |

**DTO record 规范（每条 DTO 都需遵守）：**

| 要求 | 说明 |
|------|------|
| `/// <summary>` | 每条 DTO record 头部必须有 summary 注释 |
| `[property: Description("字段说明")]` | 每个字段必填，生成 OpenAPI Schema 描述 |
| `[property: JsonPropertyName("snake_case")]` | 每个字段必填，控制 JSON 序列化字段名 |
| 格式化 | 每个字段独占一行，属性注解在上方，字段名在下方对齐 |

```csharp
/// <summary>用户登录请求</summary>
public record LoginRequest(
    [property: Description("用户名（至少 5 位）")]
    [property: JsonPropertyName("username")]
    string Username,

    [property: Description("密码（明文或 RSA-OAEP 加密）")]
    [property: JsonPropertyName("password")]
    string Password,
    ...
);
```

**Entities 属性**：使用 `[SugarColumn(ColumnDescription = "字段说明")]`，CodeFirst 建表时生成 MySQL COMMENT。

**`<param>` vs `[property: Description]` 的区别：**
- 方法入参 → XML `<param>`
- record 构造参数 → `[property: Description]`（只有它才生成 OpenAPI Schema）
- HTTP 端点方法 + 端点类扩展方法 → XML `<param>`
- Service 方法 → XML `<param>` + `<returns>`

> 脚手架模板 `templates/dotnet-service/` 已内置注释样板，`/scaffold-dotnet` 生成后按此规范补齐即可。

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
  "status": "active | planning | archived | completed",
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

**本地开发端口应与部署端口保持一致**，一个项目一个端口号，避免混淆：
- `launchSettings.json`（后端）的 `applicationUrl` → 设为分配端口
- `vite.config.ts`（前端）的 `server.port` → 设为分配端口

示例：Auth 部署端口 8100，`launchSettings.json` 也是 8100。
