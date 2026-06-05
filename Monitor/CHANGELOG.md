# CHANGELOG.md — Monitor 更新日志

## v0.2.0 (2026-06-05)

- **feat**: Redis 缓存 — 实时指标缓存优先（60s TTL），Worker 写入时同步更新
- **feat**: 历史数据时间聚合 — ≤6h 原始数据，>6h 按时间桶 AVG/MAX 聚合，最多 200 点
- **feat**: 数据库索引 — 所有查询字段建索引（7 个索引），启动时 IndexMigration 自动补建
- **feat**: Worker 采集 Windows/Linux 双兼容 — PerformanceCounter + Kernel32 P/Invoke
- **feat**: 前端响应式 — 三端自适应（手机/平板/桌面），汉堡菜单 + 滑动侧边栏
- **feat**: 翻牌时钟 — 顶栏居中，秒级翻页动画
- **feat**: ECharts time axis — 替换 category axis，滑动窗口 + 真实时间比例
- **feat**: 时间维度选择器 — 1h/6h/24h/3d/7d/30d，固定时间槽
- **feat**: ESLint + TypeScript 检查 — build 前强制 lint + typecheck
- **feat**: Docker 部署 — Dockerfile + docker-compose + CI/CD 集成
- **feat**: 全局北京时间转换器 — DateTimeBjtConverter，DB 存 UTC，API 返回 +08:00
- **feat**: 工具函数 — formatBytes（字节格式化） + 磁盘/内存详情副标题
- **fix**: SqlSugar AddSingleton → AddScoped（并发连接安全）
- **fix**: 实体字段 IsNullable 补全 + CodeFirst 跳过逻辑移除
- **fix**: alerts/history LEFT JOIN → 分开查询
- **style**: 侧边栏浅色系 #E2E8F0 + 品牌色圆点
- **style**: 卡片居中 + 三色阈值颜色条 + 磁盘放最后
- **docs**: STYLE_GUIDE 新增时序图表强制规范 + 数据库索引规范 + 前端代码质量规范
- **refactor**: Auth/Monitor 端口统一 8100（去除容器内 8080）
- **refactor**: 端点标签中英文化（Server/Docker/Uptime 等）
- **refactor**: Auth ISqlSugarClient AddScoped + CodeFirst 用 CreateScope

## v0.1.0 (2026-06-04)

- **feat**: 后端骨架 — SqlSugar + CodeFirst 自动建表（7 张实体表）
- **feat**: 后端端点 — 21 个 API 端点（服务器/Docker/站点/健康/告警）
- **feat**: 后端业务服务 — MonitorService 完整 CRUD 操作
- **feat**: Worker 采集 — ServerMetricsCollector（/proc 采集 CPU/内存/磁盘/网络）+ SitePoller（HTTP 探测）+ HealthCheckCollector（服务健康检查）
- **feat**: 前端骨架 — Vue 3 + TypeScript + UnoCSS + ECharts + Pinia（7 页面 + 6 组件）
- **feat**: 前端自动刷新 — Pinia store 每 10s 轮询所有数据
- **feat**: 前端 API 层 — axios 封装 + 完整 TypeScript 类型定义
- **feat**: 前端 Docker 部署 — Dockerfile + nginx.conf（SPA fallback + API 反向代理）
- **feat**: .NET 后端模板更新 — 注释规范 + 安全响应头 + ForwardedHeaders + Produces 规范
- **docs**: 注释规范 — Service 方法加 `<param>` `<returns>`，DTO 加 `/// <summary>` + `[JsonPropertyName]`
- **docs**: STYLE_GUIDE.md — 新增 XML 文档注释规范 + Program.cs 入口模板
- **fix**: ServerMetricsCollector 注册范围 scoped → singleton（CPU delta 跨周期累积）
- **fix**: HealthCheckCollector Auth 端口 8100 → 8080（Docker 容器内端口）
