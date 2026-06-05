using System.Security.Claims;
using AuthApi.Models;
using AuthApi.Models.Entities;
using AuthApi.Pages;
using AuthApi.Services;
using AuthShared;
using SqlSugar;

namespace AuthApi.Endpoints;

/// <summary>Auth 鉴权服务 API 端点注册</summary>
public static class AuthEndpoints
{
    /// <summary>注册 Auth 全部 API 端点</summary>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // ======== 文档 ========

        app.MapGet("/api/v1/docs", () => Results.Redirect("/scalar/v1"))
           .WithTags("Docs")
           .WithSummary("跳转到 Scalar API 文档页面")
           .RequireScope("auth:read");

        // ======== 页面 ========

        app.MapGet("/", () => Results.Redirect("/scalar/v1", permanent: false))
           .WithTags("Docs")
           .WithSummary("首页跳转到 API 文档");

        app.MapGet("/login", () => Results.Content(LoginPage.GetHtml(), "text/html; charset=utf-8"))
           .WithTags("Pages")
           .WithSummary("统一鉴权登录页")
           .WithDescription("返回登录页面 HTML。支持 ?redirect= 参数，登录成功后跳转到指定来源站点。\n如已登录且带 redirect 参数，页面会自动跳转，无需再次登录。");

        // ======== 基础运维 ========

        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "jason-auth", time = DateTime.UtcNow }))
           .WithTags("Ops")
           .WithSummary("公开健康检查")
           .WithDescription("负载均衡器和监控系统使用，无需认证。返回服务状态、服务名称、当前时间。");

        app.MapGet("/api/v1/auth/health", (ISqlSugarClient db) =>
           {
               var dbOk = false;
               try { db.Ado.GetScalar("SELECT 1"); dbOk = true; } catch { }
               return Results.Ok(new { status = "healthy", service = "jason-auth", database = dbOk, time = DateTime.UtcNow });
           })
           .WithTags("Ops")
           .WithSummary("深度健康检查")
           .WithDescription("含 MySQL 数据库连通性检测，需要 auth:read 权限。")
           .RequireScope("auth:read")
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden);

        // ======== 认证 ========

        app.MapPost("/api/v1/auth/login", async (LoginRequest request, AuthService auth, JwtService jwt, HttpContext context) =>
           {
               var remoteIp = context.Request.Headers["X-Real-IP"].FirstOrDefault()
                   ?? context.Connection.RemoteIpAddress?.ToString()
                   ?? "unknown";
               var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";

               var password = request.Password;
               if (request.Encrypted && !string.IsNullOrEmpty(password))
               {
                   try { password = jwt.Decrypt(Convert.FromBase64String(password)); } catch { }
               }

               if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Trim().Length < 5)
                   return Results.Json(new { message = "用户名或密码错误" }, statusCode: 401);
               if (string.IsNullOrEmpty(password) || password.Length < 8)
                   return Results.Json(new { message = "用户名或密码错误" }, statusCode: 401);

               var result = await auth.Login(new LoginRequest(request.Username, password), remoteIp, userAgent);

               if (result == null || string.IsNullOrEmpty(result.AccessToken))
               {
                   var response = new Dictionary<string, object> { ["message"] = "用户名或密码错误" };
                   if (result != null)
                   {
                       if (result.LockedRemainingSeconds > 0)
                           response["message"] = $"账户已锁定，请 {result.LockedRemainingSeconds / 60} 分钟后重试";
                       else if (result.RemainingAttempts > 0 && result.RemainingAttempts < 10)
                           response["message"] = $"用户名或密码错误，还剩 {result.RemainingAttempts} 次尝试";
                       response["remainingAttempts"] = result.RemainingAttempts;
                       response["lockedRemainingSeconds"] = result.LockedRemainingSeconds;
                   }
                   return Results.Json(response, statusCode: 401);
               }

               context.Response.Cookies.Append("jwt", result.AccessToken, new CookieOptions
               {
                   Domain = ".lujiesheng.cn",
                   HttpOnly = true,
                   Secure = true,
                   SameSite = SameSiteMode.Lax,
                   MaxAge = TimeSpan.FromSeconds(result.ExpiresIn),
                   Path = "/"
               });

               return Results.Ok(result);
           })
           .WithTags("Auth")
           .WithSummary("用户密码登录")
           .WithDescription("用户通过用户名和密码登录。\n\n密码传输支持两种方式：\n  • 明文（encrypted=false）— 直接提交密码原文\n  • RSA-OAEP 加密（encrypted=true）— 前端用公钥加密，后端解密\n\n登录成功后自动写入 HttpOnly Cookie（Domain=.lujiesheng.cn），各子项目共享登录态。\n\n受登录频率限制（5 次/分钟/IP），超限返回 429。连续 10 次失败后账户锁定 15 分钟。")
           .RequireRateLimiting("LoginPolicy")
           .Produces<LoginResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status429TooManyRequests);

        // ======== 服务间调用 ========

        app.MapPost("/api/v1/auth/token", async (TokenRequest request, AuthService auth) =>
           {
               var result = await auth.AuthenticateService(request);
               return result == null
                   ? Results.Json(new { message = "Client ID 或 Secret 错误" }, statusCode: 401)
                   : Results.Ok(result);
           })
           .WithTags("Service")
           .WithSummary("服务间调用认证")
           .WithDescription("后端服务使用 ClientId + ClientSecret 换取 JWT Token，用于服务之间的安全通信。\nToken 有效期为 1 小时，需定期刷新。")
           .Produces<JwtTokenResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized);

        // ======== 令牌管理 ========

        app.MapPost("/api/v1/auth/refresh", async (RefreshRequest request, AuthService auth) =>
           {
               var result = await auth.RefreshToken(request.RefreshToken);
               return result == null
                   ? Results.Json(new { message = "刷新令牌无效或已过期" }, statusCode: 401)
                   : Results.Ok(result);
           })
           .WithTags("Tokens")
           .WithSummary("刷新 AccessToken")
           .WithDescription("使用 RefreshToken 换取新的 AccessToken 和 RefreshToken。\n旧 RefreshToken 立即失效（一次性使用）。\nRefreshToken 有效期为 30 天。")
           .Produces<LoginResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized);

        // ======== 密钥接口 ========

        app.MapGet("/api/v1/auth/public-key", (JwtService jwt) =>
            Results.Content(jwt.PublicKeyPem, "text/plain; charset=utf-8"))
           .WithTags("Keys")
           .WithSummary("获取 RSA 公钥")
           .WithDescription("返回 PEM 格式的 RSA 公钥，供前端加密登录密码使用。\n\n返回值示例：\n-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...\n-----END PUBLIC KEY-----");

        // ======== 用户信息 ========

        app.MapGet("/api/v1/auth/me", async (HttpContext context, ISqlSugarClient db) =>
           {
               var user = context.User;
               if (user.Identity?.IsAuthenticated != true)
                   return Results.Json(new { message = "未登录" }, statusCode: 401);

               var userId = user.FindFirstValue("sub");
               var authUser = int.TryParse(userId, out var uid)
                   ? await db.Queryable<AuthUser>().FirstAsync(u => u.Id == uid)
                   : null;

               return Results.Json(new
               {
                   userId = userId ?? "",
                   username = user.FindFirstValue("username") ?? "",
                   nickname = authUser?.Nickname ?? "",
                   email = authUser?.Email ?? "",
                   phone = authUser?.Phone ?? "",
                   avatarUrl = authUser?.AvatarUrl ?? "",
                   bio = authUser?.Bio ?? "",
                   role = user.FindFirstValue("role") ?? "",
                   scopes = user.FindFirstValue("scopes") ?? "",
                   lastLoginAt = authUser?.LastLoginAt,
                   lastLoginIp = authUser?.LastLoginIp,
                   lastLoginCity = authUser?.LastLoginCity,
                   createdAt = authUser?.CreatedAt
               });
           })
           .WithTags("Users")
           .WithSummary("获取当前登录用户信息")
           .WithDescription("从 HttpOnly Cookie 或 Authorization Header 中解析 JWT，返回当前用户信息，含昵称/邮箱/头像等详细资料。\n各子项目（Portfolio、Monitor 等）通过此接口判断用户登录状态。\n\n未登录返回 401，前端根据响应显示「用户信息」或「登录」按钮。")
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status401Unauthorized);

        // ======== 退出 ========

        app.MapGet("/logout", (HttpContext context) =>
           {
               context.Response.Cookies.Append("jwt", "", new CookieOptions
               {
                   Domain = ".lujiesheng.cn",
                   HttpOnly = true,
                   Secure = true,
                   SameSite = SameSiteMode.Lax,
                   Expires = DateTimeOffset.UnixEpoch,
                   Path = "/"
               });

               var redirect = context.Request.Query["redirect"].FirstOrDefault();
               if (!string.IsNullOrEmpty(redirect))
               {
                   try
                   {
                       var uri = new Uri(redirect, UriKind.Absolute);
                       var host = uri.Host;
                       if (host == "lujiesheng.cn" || host.EndsWith(".lujiesheng.cn"))
                           return Results.Redirect(redirect);
                   }
                   catch { }
                   return Results.Redirect("https://lujiesheng.cn");
               }

               return Results.Json(new { message = "已退出登录" });
           })
           .WithTags("Users")
           .WithSummary("退出登录")
           .WithDescription("清除 JWT HttpOnly Cookie，可选 ?redirect= 参数跳转到指定页面。\n\nredirect 参数仅允许跳转到 *.lujiesheng.cn 域名，防止开放重定向攻击。\n未传入 redirect 时返回 JSON 成功消息。")
           .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status302Found);
    }
}
