using Microsoft.AspNetCore.HttpOverrides;
using __ProjectName__Api.Endpoints;
using __ProjectName__Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ======== 服务注册 ========

// OpenAPI / Scalar
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "__ProjectName__ API";
        document.Info.Description = """
            __ProjectName__ 服务

            功能描述
            """;
        document.Info.Version = $"v0.1.0 (.NET {Environment.Version})";
        return Task.CompletedTask;
    });
});

// 服务注册
// builder.Services.AddScoped<__ProjectName__Service>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://lujiesheng.cn")
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
    options.WithTitle("__ProjectName__ API · API 文档")
           .WithTheme(ScalarTheme.BluePlanet)
           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
           .WithFavicon("/images/favicon.png");
});

app.Map__ProjectName__Endpoints();

app.Run();
