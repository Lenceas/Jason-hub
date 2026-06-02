namespace __ProjectName__Api.Endpoints;

/// <summary>__ProjectName__ API 端点注册</summary>
public static class __ProjectName__Endpoints
{
    public static void Map__ProjectName__Endpoints(this WebApplication app)
    {
        // ======== 基础运维 ========

        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "__project-name__", time = DateTime.UtcNow }))
           .WithTags("运维")
           .WithSummary("公开健康检查")
           .WithDescription("负载均衡器和监控系统使用，无需认证。");

        // ======== 业务端点 ========

        // TODO: 按业务模块分组添加端点

        // 示例端点
        app.MapGet("/api/v1/__project-name__/ping", () => Results.Ok(new { message = "pong" }))
           .WithTags("示例")
           .WithSummary("连通性测试")
           .WithDescription("服务连通性检测端点。");
    }
}
