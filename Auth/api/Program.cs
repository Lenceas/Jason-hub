using System.Text;
using System.Threading.RateLimiting;
using AuthApi.Models;
using AuthApi.Services;
using AuthShared;
using Microsoft.AspNetCore.CookiePolicy;
using Scalar.AspNetCore;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// ======== 服务注册 ========

// OpenAPI / Scalar
builder.Services.AddOpenApi();

// SqlSugar
builder.Services.AddSingleton<ISqlSugarClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("Default")
        ?? Environment.GetEnvironmentVariable("AUTH_DB_CONNECTION")
        ?? throw new InvalidOperationException("缺少数据库连接配置：请设置 AUTH_DB_CONNECTION 环境变量或 ConnectionStrings:Default");
    return new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = connStr,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true
    });
});

// JWT
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton(sp => new JwtValidator(sp.GetRequiredService<JwtService>().PublicKeyPem));

// Auth Handler (Shared middleware)
builder.Services.AddSingleton<AuthHandler>();

// Auth Service
builder.Services.AddScoped<AuthService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "https://lujiesheng.cn",
                "https://www.lujiesheng.cn",
                "https://monitor.lujiesheng.cn"
            )
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// Cookie
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.Secure = CookieSecurePolicy.Always;
});

var app = builder.Build();

// ======== 中间件 ========
app.UseCors();
app.UseRateLimiter();
app.UseCookiePolicy();
app.Use(AuthHandlerMiddleware);

// ======== 端点 ========

// Scalar API 文档（需 JWT）
app.MapGet("/api/v1/docs", () => Results.Redirect("/scalar/v1")).RequireScope("auth:read");

// OpenAPI JSON
app.MapOpenApi();

// Scalar UI
app.MapScalarApiReference("scalar/v1", options =>
{
    options.WithTitle("Jason-hub Auth API")
           .WithTheme(ScalarTheme.BluePlanet)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

// 登录页 HTML
app.MapGet("/login", () =>
{
    var html = """
    <!DOCTYPE html>
    <html lang="zh-CN">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>登录 — Jason-hub Auth</title>
        <style>
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body {
                font-family: -apple-system, "Microsoft YaHei", sans-serif;
                background: linear-gradient(135deg, #f0f4ff 0%, #e8f0fe 100%);
                min-height: 100vh; display: flex; align-items: center; justify-content: center;
            }
            .card {
                background: rgba(255,255,255,0.9);
                backdrop-filter: blur(12px);
                padding: 2.5rem; border-radius: 16px;
                box-shadow: 0 8px 32px rgba(59,130,246,0.1);
                width: 380px; max-width: 90vw;
            }
            h1 { color: #1E293B; font-size: 1.5rem; text-align: center; margin-bottom: 0.5rem; }
            .subtitle { color: #64748B; text-align: center; font-size: 0.875rem; margin-bottom: 2rem; }
            .form-group { margin-bottom: 1.25rem; }
            label { display: block; color: #475569; font-size: 0.875rem; margin-bottom: 0.375rem; }
            input {
                width: 100%; padding: 0.75rem 1rem; border: 1px solid #E2E8F0;
                border-radius: 8px; font-size: 0.9375rem; transition: border-color 0.2s;
                outline: none;
            }
            input:focus { border-color: #3B82F6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1); }
            button {
                width: 100%; padding: 0.75rem; background: #3B82F6; color: white;
                border: none; border-radius: 8px; font-size: 1rem; font-weight: 500;
                cursor: pointer; transition: background 0.2s;
            }
            button:hover { background: #2563EB; }
            .error { color: #EF4444; font-size: 0.875rem; text-align: center; margin-top: 1rem; display: none; }
            .brand { color: #3B82F6; font-weight: 700; }
        </style>
    </head>
    <body>
        <div class="card">
            <h1>Jason-hub <span class="brand">Auth</span></h1>
            <p class="subtitle">统一鉴权中心</p>
            <div id="error" class="error"></div>
            <form id="loginForm">
                <div class="form-group">
                    <label for="username">用户名</label>
                    <input type="text" id="username" name="username" placeholder="请输入用户名" autocomplete="username" required>
                </div>
                <div class="form-group">
                    <label for="password">密码</label>
                    <input type="password" id="password" name="password" placeholder="请输入密码" autocomplete="current-password" required>
                </div>
                <button type="submit">登 录</button>
            </form>
        </div>
        <script>
            document.getElementById('loginForm').addEventListener('submit', async (e) => {
                e.preventDefault();
                const error = document.getElementById('error');
                const username = document.getElementById('username').value;
                const password = document.getElementById('password').value;
                try {
                    const res = await fetch('/api/v1/auth/login', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ username, password })
                    });
                    const data = await res.json();
                    if (!res.ok) {
                        error.textContent = data.message || '登录失败';
                        error.style.display = 'block';
                        return;
                    }
                    const params = new URLSearchParams(window.location.search);
                    const redirect = params.get('redirect') || '/';
                    window.location.href = redirect + (redirect.includes('?') ? '&' : '?') + 'token=' + data.accessToken;
                } catch (err) {
                    error.textContent = '网络错误，请稍后再试';
                    error.style.display = 'block';
                }
            });
        </script>
    </body>
    </html>
    """;
    return Results.Content(html, "text/html; charset=utf-8");
});

// 公开健康检查
app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "jason-auth", time = DateTime.UtcNow }));

// 深度健康检查（需 JWT）
app.MapGet("/api/v1/auth/health", (ISqlSugarClient db) =>
{
    var dbOk = false;
    try { db.Ado.GetScalar("SELECT 1"); dbOk = true; } catch { }
    return Results.Ok(new {
        status = "healthy",
        service = "jason-auth",
        database = dbOk,
        time = DateTime.UtcNow
    });
}).RequireScope("auth:read");

// 用户登录
app.MapPost("/api/v1/auth/login", async (LoginRequest request, AuthService auth, HttpContext context) =>
{
    var result = await auth.Login(request);
    if (result == null)
        return Results.Json(new { message = "用户名或密码错误，或账户已锁定" }, statusCode: 401);

    // 写 HttpOnly Cookie（跨子域共享）
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
}).RequireRateLimiting("LoginPolicy");

// 服务间调用认证
app.MapPost("/api/v1/auth/token", async (TokenRequest request, AuthService auth) =>
{
    var result = await auth.AuthenticateService(request);
    return result == null
        ? Results.Json(new { message = "Client ID 或 Secret 错误" }, statusCode: 401)
        : Results.Ok(result);
});

// 刷新 Token
app.MapPost("/api/v1/auth/refresh", async (RefreshRequest request, AuthService auth) =>
{
    var result = await auth.RefreshToken(request.RefreshToken);
    return result == null
        ? Results.Json(new { message = "刷新令牌无效或已过期" }, statusCode: 401)
        : Results.Ok(result);
});

// 公钥端点
app.MapGet("/api/v1/auth/public-key", (JwtService jwt) =>
    Results.Content(jwt.PublicKeyPem, "text/plain; charset=utf-8"));

app.Run();

// ======== 中间件 ========
static async Task AuthHandlerMiddleware(HttpContext context, RequestDelegate next)
{
    var path = context.Request.Path.Value ?? "";
    if (path is "/healthz" or "/login" or "/api/v1/auth/public-key" or "/api/v1/auth/login" or "/api/v1/auth/token")
    {
        await next(context);
        return;
    }

    var handler = context.RequestServices.GetRequiredService<AuthHandler>();
    await handler.InvokeAsync(context, next);
}
