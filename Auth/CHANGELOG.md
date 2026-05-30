# Auth 鉴权服务 — 变更日志

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
- **db**: 初始管理员用户（jason）
