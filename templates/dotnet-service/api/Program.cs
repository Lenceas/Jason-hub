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

app.UseCors();

// ======== 端点 ========

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
