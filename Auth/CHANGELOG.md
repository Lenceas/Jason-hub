# Auth 鉴权服务 — 变更日志

---

## v0.3.0 (2026-06-01)

- **feat**: RSA-OAEP 前端加密密码 — Web Crypto API 加密，后端 JwtService 解密
- **feat**: 安全响应头中间件（X-Content-Type-Options / X-Frame-Options / HSTS 等）
- **feat**: ForwardedHeaders 中间件 — 信任 Nginx 反代头，获取真实客户端 IP
- **feat**: 登录审计日志升级 — 每条日志含 `[IP] [UA] [User]`，真实外网 IP + User-Agent
- **feat**: 服务端输入校验（用户名 ≥ 5 位、密码 ≥ 8 位），防绕过前端直接调 API
- **feat**: 失焦校验 + 提交校验 — 按顺序先用户名后密码，空值不在失焦时提示
- **feat**: 锁定倒计时 — 账户锁定时页面实时倒数，输入框和按钮全部禁用
- **feat**: 登录来源识别 — 检测 `?redirect=` 参数，显示来源子项目名称
- **feat**: 重定向安全校验 — 仅允许 `*.lujiesheng.cn` 域名，防 Open Redirect 攻击
- **feat**: 输入框 X 清空按钮 — 用户名和密码各一个，有内容时显示
- **feat**: 错误提示自动消失（2s）+ 按顺序校验拦截
- **feat**: 登录页面 favicon 与 Portfolio 主站统一
- **feat**: 动感渐变背景 + 三色渐变装饰圆 + 柔和漂移动画
- **feat**: RSA 限流分区键改用 X-Real-IP（经反代后获取真实 IP）
- **security**: 统一错误响应 — 不区分"用户不存在"和"密码错误"，防用户名枚举
- **style**: 登录页文案简化（标题/placeholder/按钮）、必填红色 `*`、label 上下结构
- **fix**: 输入框 CSS 类名冲突 — `.error` 和 `.error-msg` 分离，避免 display:none 覆盖
- **fix**: JwtService 弃用警告 — `OperatingSystem.IsWindows()` 替代 try/catch

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
