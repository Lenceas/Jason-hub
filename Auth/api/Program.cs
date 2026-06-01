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
                padding: 1rem;
            }
            @keyframes fadeUp {
                from { opacity: 0; transform: translateY(24px); }
                to   { opacity: 1; transform: translateY(0); }
            }
            .card {
                background: rgba(255,255,255,0.9);
                backdrop-filter: blur(12px);
                padding: 2.5rem; border-radius: 16px;
                box-shadow: 0 8px 32px rgba(59,130,246,0.1);
                width: 380px; max-width: 100%;
                animation: fadeUp 0.5s ease-out;
            }
            h1 { color: #1E293B; font-size: 1.5rem; text-align: center; margin-bottom: 0.5rem; }
            .subtitle { color: #64748B; text-align: center; font-size: 0.875rem; margin-bottom: 2rem; }
            .form-group { margin-bottom: 1.25rem; }
            label {
                display: block; color: #475569; font-size: 0.875rem;
                margin-bottom: 0.375rem; font-weight: 500;
            }
            .input-wrap {
                position: relative; display: flex; align-items: center;
            }
            .input-wrap input {
                width: 100%; padding: 0.75rem 1rem; border: 1px solid #E2E8F0;
                border-radius: 8px; font-size: 0.9375rem; transition: border-color 0.2s, box-shadow 0.2s;
                outline: none; background: #fff;
            }
            .input-wrap input:focus {
                border-color: #3B82F6; box-shadow: 0 0 0 3px rgba(59,130,246,0.1);
            }
            .input-wrap input.error {
                border-color: #EF4444; box-shadow: 0 0 0 3px rgba(239,68,68,0.1);
            }
            .pw-toggle {
                position: absolute; right: 12px; top: 50%; transform: translateY(-50%);
                background: none; border: none; cursor: pointer; padding: 4px;
                color: #94A3B8; display: flex; align-items: center; justify-content: center;
                transition: color 0.2s; width: auto;
            }
            .pw-toggle:hover { color: #475569; background: none; }
            .pw-toggle svg { width: 20px; height: 20px; display: block; }
            button[type="submit"] {
                width: 100%; padding: 0.75rem; background: #3B82F6; color: white;
                border: none; border-radius: 8px; font-size: 1rem; font-weight: 500;
                cursor: pointer; transition: background 0.2s, transform 0.15s;
                display: flex; align-items: center; justify-content: center; gap: 0.5rem;
                position: relative;
            }
            button[type="submit"]:hover:not(:disabled) { background: #2563EB; }
            button[type="submit"]:active:not(:disabled) { transform: scale(0.98); }
            button[type="submit"]:disabled { background: #93C5FD; cursor: not-allowed; }
            .spinner {
                width: 18px; height: 18px; border: 2px solid rgba(255,255,255,0.3);
                border-top-color: #fff; border-radius: 50%; animation: spin 0.6s linear infinite;
                display: none;
            }
            button[type="submit"].loading .spinner { display: inline-block; }
            button[type="submit"].loading .btn-text { display: none; }
            @keyframes spin { to { transform: rotate(360deg); } }
            .error {
                color: #EF4444; font-size: 0.875rem; text-align: center;
                margin-top: 1rem; display: none; padding: 0.5rem 0.75rem;
                background: #FEF2F2; border-radius: 8px; border: 1px solid #FECACA;
            }
            .brand { color: #3B82F6; font-weight: 700; }

            @media (max-width: 480px) {
                .card { padding: 1.75rem 1.25rem; }
                h1 { font-size: 1.25rem; }
                input { padding: 0.65rem 0.875rem; font-size: 0.875rem; }
                button[type="submit"] { padding: 0.65rem; font-size: 0.9375rem; }
                .subtitle { margin-bottom: 1.5rem; }
                .form-group { margin-bottom: 1rem; }
            }
        </style>
    </head>
    <body>
        <div class="card">
            <h1>Jason-hub <span class="brand">Auth</span></h1>
            <p class="subtitle">统一鉴权中心</p>
            <div id="error" class="error"></div>
            <form id="loginForm" novalidate>
                <div class="form-group">
                    <label for="username">用户名</label>
                    <div class="input-wrap">
                        <input type="text" id="username" name="username"
                               placeholder="请输入用户名" autocomplete="username"
                               autofocus required>
                    </div>
                </div>
                <div class="form-group">
                    <label for="password">密码</label>
                    <div class="input-wrap">
                        <input type="password" id="password" name="password"
                               placeholder="请输入密码" autocomplete="current-password" required>
                        <button type="button" class="pw-toggle" id="pwToggle"
                                aria-label="切换密码可见性" tabindex="-1">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"
                                 stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <path id="eyeIcon" d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                                <circle id="eyePupil" cx="12" cy="12" r="3"/>
                            </svg>
                        </button>
                    </div>
                </div>
                <button type="submit" id="submitBtn">
                    <span class="spinner"></span>
                    <span class="btn-text">登 录</span>
                </button>
            </form>
        </div>
        <script>
            (() => {
                const form = document.getElementById('loginForm');
                const error = document.getElementById('error');
                const submitBtn = document.getElementById('submitBtn');
                const username = document.getElementById('username');
                const password = document.getElementById('password');
                const pwToggle = document.getElementById('pwToggle');

                // 密码显隐切换
                pwToggle.addEventListener('click', () => {
                    const isPw = password.type === 'password';
                    password.type = isPw ? 'text' : 'password';
                });

                // 输入时清除错误
                const clearError = () => {
                    error.style.display = 'none';
                    username.classList.remove('error');
                    password.classList.remove('error');
                };
                username.addEventListener('input', clearError);
                password.addEventListener('input', clearError);

                // 登录提交
                form.addEventListener('submit', async (e) => {
                    e.preventDefault();
                    clearError();
                    submitBtn.classList.add('loading');
                    submitBtn.disabled = true;

                    try {
                        const res = await fetch('/api/v1/auth/login', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({
                                username: username.value.trim(),
                                password: password.value
                            })
                        });
                        const data = await res.json();

                        if (!res.ok) {
                            const msg = data.message || '登录失败';
                            // 区分错误类型
                            if (msg.includes('锁定')) {
                                error.textContent = '🔒 ' + msg;
                            } else if (res.status === 429) {
                                error.textContent = '⏱ 登录过于频繁，请一分钟后再试';
                            } else {
                                error.textContent = '✕ ' + msg;
                            }
                            error.style.display = 'block';
                            username.classList.add('error');
                            password.classList.add('error');
                            return;
                        }

                        // 登录成功 — 跳转
                        const params = new URLSearchParams(window.location.search);
                        const redirect = params.get('redirect') || '/';
                        const sep = redirect.includes('?') ? '&' : '?';
                        window.location.href = redirect + sep + 'token=' + data.accessToken;
                    } catch (err) {
                        error.textContent = '⚠ 网络错误，请检查网络连接后重试';
                        error.style.display = 'block';
                    } finally {
                        submitBtn.classList.remove('loading');
                        submitBtn.disabled = false;
                    }
                });
            })();
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
