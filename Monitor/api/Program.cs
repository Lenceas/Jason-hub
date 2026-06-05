using Microsoft.AspNetCore.HttpOverrides;
using MonitorApi.Endpoints;
using MonitorApi.Models.Entities;
using MonitorApi.Services;
using MonitorApi.Worker;
using MonitorApi.Worker.Collectors;
using Scalar.AspNetCore;
using SqlSugar;
using StackExchange.Redis;

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
        document.Info.Title = "Jason-hub Monitor API";
        document.Info.Description = """
            Jason-hub 统一监控平台

            提供服务器指标采集、Docker 容器管理、站点可用性探测、应用健康检查、告警规则管理、CI/CD 流水线追踪。
            后台 Agent 定时采集，实时数据存 Redis，历史数据存 MySQL。
            """;
        document.Info.Version = $"v0.1.0 (.NET {Environment.Version})";
        return Task.CompletedTask;
    });
});

// SqlSugar
builder.Services.AddScoped<ISqlSugarClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = (string.IsNullOrEmpty(config.GetConnectionString("Default"))
        ? Environment.GetEnvironmentVariable("MONITOR_DB_CONNECTION")
        : config.GetConnectionString("Default"))
        ?? throw new InvalidOperationException("缺少数据库连接配置：请设置 MONITOR_DB_CONNECTION 环境变量或 ConnectionStrings:Default");
    return new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = connStr,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true
    });
});

// Redis 缓存
var redisConnStr = builder.Configuration.GetConnectionString("Redis")
    ?? Environment.GetEnvironmentVariable("MONITOR_REDIS_CONNECTION")
    ?? "127.0.0.1:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var opts = ConfigurationOptions.Parse(redisConnStr);
    opts.AbortOnConnectFail = false; // Redis 不可用时仍可启动
    return ConnectionMultiplexer.Connect(opts);
});
builder.Services.AddSingleton<RedisCacheService>();

// 服务注册
builder.Services.AddScoped<MonitorService>();

// Worker 后台采集
builder.Services.AddHostedService<MonitorWorker>();
builder.Services.AddSingleton<ServerMetricsCollector>();
builder.Services.AddScoped<SitePoller>();
builder.Services.AddScoped<HealthCheckCollector>();
builder.Services.AddHttpClient();

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

// ======== 端点 ========

app.MapGet("/api/v1/docs", () => Results.Redirect("/scalar/v1"))
   .WithTags("文档")
   .WithSummary("跳转到 Scalar API 文档页面");

app.MapOpenApi();
app.MapScalarApiReference("scalar/v1", options =>
{
    options.WithTitle("Jason-hub Monitor API · API 文档")
           .WithTheme(ScalarTheme.BluePlanet)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
           .WithFavicon("/images/favicon.png");
});

app.MapMonitorEndpoints();

// ======== CodeFirst — 自动建表/加列 ========
// 仅处理新表和新增列，不处理索引（见下方 IndexMigration）
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
    db.CodeFirst.InitTables(
        typeof(MonitorServerMetric),
        typeof(MonitorSite),
        typeof(MonitorUptimeRecord),
        typeof(MonitorContainerSnapshot),
        typeof(MonitorHealthRecord),
        typeof(MonitorAlertRule),
        typeof(MonitorAlertEvent)
    );

    // ======== 索引迁移 — 补建缺失索引（不丢数据）= ========
    var indexes = new (string table, string name, string cols)[]
    {
        ("server_metrics",        "idx_ts",              "Ts"),
        ("container_snapshots",   "idx_ts",              "Ts"),
        ("uptime_records",        "idx_site_checked",    "SiteId, CheckedAt"),
        ("health_records",        "idx_service_ts",      "Service, Ts"),
        ("alert_events",          "idx_ruleid",          "RuleId"),
        ("alert_events",          "idx_triggered",       "TriggeredAt"),
    };
    foreach (var (table, name, cols) in indexes)
    {
        try { db.Ado.ExecuteCommand($"CREATE INDEX IF NOT EXISTS {name} ON {table} ({cols})"); }
        catch { /* 已存在则跳过 */ }
    }
}

app.Run();
