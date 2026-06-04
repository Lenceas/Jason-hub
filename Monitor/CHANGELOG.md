# CHANGELOG.md — Monitor 更新日志

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
