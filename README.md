# Jason-hub

> 个人项目 Monorepo · 主站聚合 · 子项目统一入口

通过 [Portfolio](./Portfolio) 主站聚合展示个人介绍与项目卡片，跳转到各个独立子项目。

## 结构

```
Jason-hub/
├── Portfolio/     ← Astro 6 个人主页（主站）
├── ...            ← 后续子项目 (Vue 3 + TS)
├── AGENTS.md      ← AI 协作说明
├── ARCHITECTURE.md ← 架构设计
└── STYLE_GUIDE.md ← 编码规范
```

## 端口规则

| 类型 | 范围 |
|------|------|
| 前端开发 | 8000–8049 |
| 后端 API | 8050–8099 |

Portfolio 主站使用 **8000** 端口。

## 快速开始

```bash
# 进入主站
cd Portfolio

# 安装依赖
npm install

# 本地开发（自动打开浏览器）
npm run dev

# 构建
npm run build
```

---

参见 [AGENTS.md](./AGENTS.md) · [ARCHITECTURE.md](./ARCHITECTURE.md) · [STYLE_GUIDE.md](./STYLE_GUIDE.md)
