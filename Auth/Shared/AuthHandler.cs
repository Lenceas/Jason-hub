using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace AuthShared;

/// <summary>
/// ServiceAuthHandler — 解析 JWT 填充 HttpContext.User
/// 优先读 Authorization: Bearer，回退到 Cookie: jwt
/// </summary>
public class AuthHandler : IMiddleware
{
    private readonly JwtValidator _validator;

    public AuthHandler(JwtValidator validator)
    {
        _validator = validator;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = ExtractToken(context);
        if (!string.IsNullOrEmpty(token))
        {
            var principal = _validator.ValidateToken(token);
            if (principal != null)
            {
                context.User = principal;
                context.Items["TokenType"] = JwtValidator.GetTokenType(principal);
            }
        }

        await next(context);
    }

    private static string? ExtractToken(HttpContext context)
    {
        // 1. 优先读 Authorization header
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return authHeader["Bearer ".Length..].Trim();

        // 2. 回退读 Cookie
        if (context.Request.Cookies.TryGetValue("jwt", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
            return cookieToken;

        return null;
    }
}

/// <summary>Minimal API 端点过滤器 — 声明式权限检查</summary>
public class RequireScopeFilter : IEndpointFilter
{
    private readonly string _scope;

    public RequireScopeFilter(string scope) => _scope = scope;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            return Results.Json(new { message = "未授权" }, statusCode: 401);

        if (!JwtValidator.HasScope(context.HttpContext.User, _scope))
            return Results.Json(new { message = "权限不足" }, statusCode: 403);

        return await next(context);
    }
}

/// <summary>扩展方法：链式调用 .RequireScope("xxx")</summary>
public static class AuthEndpointExtensions
{
    public static RouteHandlerBuilder RequireScope(this RouteHandlerBuilder builder, string scope)
        => builder.AddEndpointFilter(new RequireScopeFilter(scope));
}
