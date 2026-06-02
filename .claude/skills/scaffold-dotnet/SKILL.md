---
name: scaffold-dotnet
description: 从模板创建 .NET 后端子项目 — 复制模板 → 重命名 → 替换占位符 → 设置端口
---

# scaffold-dotnet — .NET 后端脚手架

在对话中说 **"创建 {项目名} 后端"** 或调用 `/scaffold-dotnet` 即可触发。

基于 `templates/dotnet-service/` 模板生成标准 .NET 后端子项目结构。

## 用法

```
/scaffold-dotnet <项目名> [端口号]
```

或直接在对话中说：
- "创建 Auth 后端"
- "创建 Monitor 后端，端口 8051"
- "给 Notification 建个后端项目"

## 流程

### ① 收集参数

从对话或上下文推断以下信息：

| 参数 | 说明 | 默认值 |
|------|------|--------|
| 项目名（必填） | PascalCase，如 `Monitor`、`Auth`、`Notification` | — |
| 端口号（可选） | API 端口，8050-8099 段 | `8050` |

端口优先级：
1. 用户对话中明确指定 → 使用指定值
2. 未指定 → 查 `AGENTS.md` / `CLAUDE.md` 端口分配表，看该项目是否有已分配的端口
3. 都没有 → 使用默认 `8050`

### ② 执行脚本

```bash
bash scripts/scaffold-dotnet.sh <项目名> [端口号]
```

### ③ 展示结果

向用户输出创建结果（脚本已自动输出目录结构），并提示下一步：

```
🎉 {项目名} 后端项目创建完成！

目录结构:
  ...
  
下一步:
  1. 在 Program.cs 中注册服务
  2. 在 Endpoints/{项目名}Endpoints.cs 中添加业务端点
  3. 在 Services/{项目名}Service.cs 中实现业务逻辑
```

**敏感信息红线**：创建完成后检查 `appsettings.json`、`Program.cs` 等文件，确认没有明文密码/Token/连接串出现。连接字符串中的密码使用 `***` 或环境变量引用。
