# Monitor Web — 监控面板前端

> Vue 3 + TypeScript + UnoCSS + ECharts + Pinia

## 开发

```bash
npm install       # 安装依赖
npm run dev       # 开发服务器 → http://localhost:8001
npm run build     # 生产构建 → dist/
```

## 代理配置

开发环境下 `/api/*` 请求自动代理到后端 `http://localhost:8051`（见 `vite.config.ts`）。

## 目录结构

```
src/
├── layouts/       ← 布局（侧边栏 + 顶栏）
├── views/         ← 7 个页面（概览/服务器/Docker/站点/健康/告警/设置）
├── components/    ← 8 个通用组件（指标卡/图表/翻牌时钟/状态标签等）
├── stores/        ← Pinia 状态管理
├── api/           ← axios + API 函数
├── styles/        ← CSS 变量（监控状态色）
├── utils/         ← 工具函数（字节格式化等）
└── types/         ← TypeScript 类型定义
```

## 环境变量

| 变量 | 默认值 | 说明 |
|------|--------|------|
| `VITE_API_BASE` | `/api/v1` | API 基础路径（Docker 部署用） |
