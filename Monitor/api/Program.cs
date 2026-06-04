using Microsoft.AspNetCore.HttpOverrides;
using MonitorApi.Endpoints;
using MonitorApi.Models.Entities;
using MonitorApi.Services;
using MonitorApi.Worker;
using MonitorApi.Worker.Collectors;
using Scalar.AspNetCore;
using SqlSugar;

var builder = WebApplication.CreateBuilder(args);

// ======== 服务注册 ========

// OpenAPI / Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Monitor API";
        document.Info.Description = """
            Monitor — Jason-hub 统一监控平台

            服务器监控、Docker 容器管理、站点可用性检测、应用健康检查、告警通知、CI/CD 流水线追踪。
            """;
        document.Info.Version = $"v0.1.0 (.NET {Environment.Version})";
        return Task.CompletedTask;
    });
});

// SqlSugar
builder.Services.AddSingleton<ISqlSugarClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("Default")
        ?? Environment.GetEnvironmentVariable("MONITOR_DB_CONNECTION")
        ?? throw new InvalidOperationException("缺少数据库连接配置：请设置 MONITOR_DB_CONNECTION 环境变量或 ConnectionStrings:Default");
    return new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = connStr,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true
    });
});

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

app.UseCors();

// ======== 端点 ========

app.MapGet("/api/v1/docs", () => Results.Redirect("/scalar/v1"))
   .WithTags("文档")
   .WithSummary("跳转到 Scalar API 文档页面");

app.MapOpenApi();
app.MapScalarApiReference("scalar/v1", options =>
{
    options.WithTitle("Monitor API · API 文档")
           .WithTheme(ScalarTheme.BluePlanet)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
           .WithFavicon("/images/favicon.png");
});

app.MapMonitorEndpoints();

// ======== CodeFirst — 自动建表/加列 ========
try
{
    var db = app.Services.GetRequiredService<ISqlSugarClient>();
    db.CodeFirst.InitTables(
        typeof(MonitorServerMetric),
        typeof(MonitorSite),
        typeof(MonitorUptimeRecord),
        typeof(MonitorContainerSnapshot),
        typeof(MonitorHealthRecord),
        typeof(MonitorAlertRule),
        typeof(MonitorAlertEvent)
    );
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "CodeFirst 初始化跳过（开发环境或无数据库连接）");
}

app.Run();
