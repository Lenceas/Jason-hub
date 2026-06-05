# templates — 项目脚手架模板

> 提供标准化的项目模板，用于快速创建新子项目。

## 模板列表

| 模板 | 说明 | 使用方式 |
|------|------|---------|
| `dotnet-service/` | .NET 10 后端子项目模板（Program.cs + Endpoints/ + Services/ + Models/） | `/scaffold-dotnet` 技能或 `scripts/scaffold-dotnet.sh` |

## 规范

- 模板目录使用 kebab-case
- 模板内代码结构遵循 [STYLE_GUIDE.md](../STYLE_GUIDE.md) 的 .NET 后端规范
- 模板内置安全响应头、ForwardedHeaders、Scalar 文档等标准配置
