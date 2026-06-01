# Auth 鉴权服务 — 变更日志

---

## v0.2.1 (2026-06-01)

- **feat**: 登录页全面优化 — 加载状态 / 密码显隐切换 / 输入时清除错误
- **feat**: 登录页入场动画（fadeUp）+ 按钮按压微动效
- **feat**: 错误提示细分（账户锁定 / 频率限制 / 网络异常）
- **style**: 移动端响应式打磨（间距/字号适配小屏）
- **ux**: 自动聚焦用户名输入框 + 禁用时按钮反馈

## v0.2.0 (2026-05-31)

- **feat**: Auth 部署配置 — docker-compose 服务编排 / `.env` 连接配置 / Nginx 反代
- **feat**: CI/CD 支持 Auth 构建部署（触发 project/auth 分支）
- **deploy**: Auth 服务正式部署上线 — 容器化运行 `:8100`
- **deploy**: SSL 证书 Let's Encrypt ECC（`api-auth.lujiesheng.cn`）
- **deploy**: `jason_auth` 数据库建表 + 初始化管理员
- **fix**: Auth Dockerfile 构建路径修复（COPY Auth/ 目录结构）
- **fix**: 移除 changelog 中的密码明文
- **docs**: 初始管理员用户 jason → admin
- **docs**: 新增敏感信息红线规范（RELEASE.md 2c 检测 / CLAUDE.md 警示）
- **security**: `/healthz` 公网 Nginx 拦截（`return 404`）

---

## v0.1.0 (2026-05-30)

- **feat**: 项目骨架搭建（.NET 10 + Minimal API）
- **feat**: Scalar API 文档集成（/scalar/v1，BluePlanet 蓝白主题）
- **feat**: OpenAPI 自动生成（/openapi/v1.json）
- **feat**: JWT 签发服务 — RS256 非对称签名，密钥持久化
- **feat**: 用户密码登录 — BCrypt 验证 + HttpOnly Cookie 跨子域共享
- **feat**: 服务间调用认证 — ClientId/Secret → JWT
- **feat**: 爆破防御 — IP 限流（5次/分钟）+ 账户锁定（10次/15分钟）
- **feat**: 登录页 HTML — 蓝白毛玻璃风格，与 Portfolio 主题统一
- **feat**: `AuthShared` 共享中间件库 — JwtValidator / AuthHandler / RequireScope
- **feat**: 健康检查端点 — `/healthz`（公开） + `/api/v1/auth/health`（需 JWT）
- **feat**: RSA 公钥端点 — `/api/v1/auth/public-key`
- **feat**: 刷新令牌机制
- **docs**: 根 PLAN.md §八 Auth 方案定义
- **docs**: Auth 子项目文档三件套（PLAN.md / CHANGELOG.md / README.md）
- **db**: `jason_auth` 库建表（auth_users / auth_clients / auth_refresh_tokens）
- **db**: 初始管理员用户（admin）
