namespace __ProjectName__Api.Endpoints;

/// <summary>__ProjectName__ API 端点注册</summary>
public static class __ProjectName__Endpoints
{
    /// <summary>映射 __ProjectName__ 全部 API 端点</summary>
    /// <param name="app">WebApplication 实例</param>
    public static void Map__ProjectName__Endpoints(this WebApplication app)
    {
        // ======== 基础运维 ========

        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "__project-name__", time = DateTime.UtcNow }))
           .WithTags("运维")
           .WithSummary("公开健康检查")
           .WithDescription("负载均衡器和监控系统使用，无需认证。返回服务状态、服务名称、当前时间。")
           .Produces(StatusCodes.Status200OK);

        // ======== 业务端点 ========

        // TODO: 按业务模块分组添加端点

        // 示例端点 — 调用 __ProjectName__Service
        app.MapGet("/api/v1/__project-name__/greet", async (__ProjectName__Service svc, string? name) =>
           {
               name ??= "世界";
               var msg = await svc.GreetAsync(name);
               return Results.Ok(new { message = msg });
           })
           .WithTags("示例")
           .WithSummary("问候接口")
           .WithDescription("返回问候语，用于验证服务连通性。参数 name 可选，默认为「世界」。")
           .Produces(StatusCodes.Status200OK);
    }
}
