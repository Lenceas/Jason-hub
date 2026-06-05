using System.Threading.RateLimiting;
using AuthApi.Endpoints;
using AuthApi.Models.Entities;
using AuthApi.Services;
using AuthShared;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// ======== JSON 序列化：UTC → 北京时间 ========
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new DateTimeBjtConverter());
});

// ======== 服务注册 ========

// OpenAPI / Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Jason-hub Auth API";
        document.Info.Description = """
            Jason-hub 统一鉴权服务

            提供用户登录、JWT 签发、令牌管理、退出登录等核心认证功能。
            支持 RSA-OAEP 前端加密密码、HttpOnly Cookie 跨子域共享、爆破防御（IP 限流 + 账户锁定）。

            各子项目（Portfolio / Monitor 等）通过此服务完成统一身份认证。
            """;
        document.Info.Version = $"v1.0.0 (.NET {Environment.Version})";
        return Task.CompletedTask;
    });
});

// SqlSugar
builder.Services.AddScoped<ISqlSugarClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = (string.IsNullOrEmpty(config.GetConnectionString("Default"))
        ? Environment.GetEnvironmentVariable("AUTH_DB_CONNECTION")
        : config.GetConnectionString("Default"))
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

// Ip2RegionService — IP 城市解析（ip2region 离线库，微秒级）
builder.Services.AddSingleton<Ip2RegionService>();

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

// Rate Limiting — 使用 X-Real-IP 作为分区键（经 Nginx 反代后获取真实 IP）
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("LoginPolicy", context =>
    {
        var ip = context.Request.Headers["X-Real-IP"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// 安全响应头
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "0");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    if (!app.Environment.IsDevelopment())
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next(context);
});

app.UseStaticFiles();
app.UseCors();
app.UseRateLimiter();
app.UseCookiePolicy();
app.Use(AuthHandlerMiddleware);

// ======== 端点 ========

app.MapOpenApi();
app.MapScalarApiReference("scalar/v1", options =>
{
    options.WithTitle("Jason-hub Auth API · API 文档")
           .WithTheme(ScalarTheme.BluePlanet)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
           .WithFavicon("/images/favicon.png");
});

app.MapAuthEndpoints();

// ======== CodeFirst — 自动建表/加列 ========
// 仅处理新表和新增列，不处理索引（见下方 IndexMigration）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
    try
    {
        db.CodeFirst.InitTables(
            typeof(AuthUser),
            typeof(AuthRefreshToken),
            typeof(AuthClient)
        );
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "CodeFirst 跳过（表已存在或字段冲突）");
    }

    // ======== 索引迁移 — 补建缺失索引（不丢数据）= ========
    var indexes = new (string table, string name, string cols)[]
    {
        ("auth_refresh_tokens", "idx_token_hash",          "token_hash"),
        ("auth_refresh_tokens", "idx_user_revoked_expires", "user_id, revoked, expires_at"),
        ("auth_clients",        "idx_client_id",            "client_id"),
    };
    foreach (var (table, name, cols) in indexes)
    {
        try { db.Ado.ExecuteCommand($"CREATE INDEX IF NOT EXISTS {name} ON {table} ({cols})"); }
        catch { /* 已存在则跳过 */ }
    }
}

app.Run();

// ======== 中间件 ========
static async Task AuthHandlerMiddleware(HttpContext context, RequestDelegate next)
{
    var path = context.Request.Path.Value ?? "";
    if (path is "/healthz" or "/login" or "/logout" or "/api/v1/auth/public-key" or "/api/v1/auth/login" or "/api/v1/auth/token")
    {
        await next(context);
        return;
    }

    var handler = context.RequestServices.GetRequiredService<AuthHandler>();
    await handler.InvokeAsync(context, next);
}
