using MonitorApi.Models;
using MonitorApi.Services;

namespace MonitorApi.Endpoints;

/// <summary>Monitor API 端点注册</summary>
public static class MonitorEndpoints
{
    /// <summary>映射 Monitor 全部 API 端点</summary>
    /// <param name="app">WebApplication 实例</param>
    public static void MapMonitorEndpoints(this WebApplication app)
    {
        // ======== 基础运维 ========

        app.MapGet("/healthz", () => Results.Ok(new { status = "healthy", service = "monitor", time = DateTime.UtcNow }))
           .WithTags("运维")
           .WithSummary("公开健康检查")
           .WithDescription("负载均衡器和监控系统使用，无需认证。返回服务状态、服务名称、当前时间。")
           .Produces(StatusCodes.Status200OK);

        // ======== 服务器监控 ========

        app.MapGet("/api/v1/server/metrics", async (MonitorService svc) =>
           {
               var metrics = await svc.GetLatestMetricsAsync();
               return metrics is null ? Results.NotFound() : Results.Ok(metrics);
           })
           .WithTags("服务器监控")
           .WithSummary("实时服务器指标")
           .WithDescription("返回最新一次采集的服务器指标数据，包含 CPU 使用率、内存使用率、磁盘使用率、网络流量和系统负载。\n\n如果尚无采集数据（Agent 首次部署），返回 404。")
           .Produces<ServerMetricsResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/api/v1/server/history", async (MonitorService svc, int? range) =>
           {
               var hours = range ?? 24;
               if (hours < 1 || hours > 168) hours = 24;
               var history = await svc.GetMetricsHistoryAsync(hours);
               return Results.Ok(new { range_hours = hours, metrics = history });
           })
           .WithTags("服务器监控")
           .WithSummary("历史指标趋势")
           .WithDescription("查询指定小时范围内的历史指标，用于渲染趋势折线图。\n\n参数 range 范围 1-168 小时（默认 24h），超出范围自动修正为 24h。\n返回按时间正序排列的指标列表。")
           .Produces(StatusCodes.Status200OK);

        // ======== Docker 容器 ========

        app.MapGet("/api/v1/docker/containers", async (MonitorService svc) =>
           {
               var containers = await svc.GetLatestContainerSnapshotsAsync();
               return Results.Ok(containers);
           })
           .WithTags("Docker 容器")
           .WithSummary("容器列表（最新快照）")
           .WithDescription("返回所有 Docker 容器的最新状态快照，含容器名称、运行状态、CPU 和内存占用。\n\n数据由后台 Agent 每 30 秒采集一次，若 Agent 未运行则返回空列表。")
           .Produces<List<ContainerSnapshotResponse>>(StatusCodes.Status200OK);

        // TODO: 容器详情 + 一键启停（需 Docker SDK 集成）

        // ======== 站点可用性 ========

        app.MapGet("/api/v1/uptime/sites", async (MonitorService svc) =>
           {
               var sites = await svc.GetSitesAsync();
               return Results.Ok(sites);
           })
           .WithTags("站点监控")
           .WithSummary("站点列表")
           .WithDescription("返回所有已配置的监控站点列表，每个站点包含名称、URL、探测间隔和超时设置。")
           .Produces<List<SiteResponse>>(StatusCodes.Status200OK);

        app.MapPost("/api/v1/uptime/sites", async (MonitorService svc, SiteRequest req) =>
           {
               if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Url))
                   return Results.BadRequest(new { error = "站点名称和URL不能为空" });
               var site = await svc.CreateSiteAsync(req);
               return Results.Created($"/api/v1/uptime/sites/{site.Id}", site);
           })
           .WithTags("站点监控")
           .WithSummary("新增监控站点")
           .WithDescription("添加一个需要监控可用性的站点。Agent 将会定时探测该站点的 HTTP 状态码和响应时间。\n\n必填字段：name（站点名称）、url（完整 URL，需含 http/https 协议）。\n可选字段：interval_sec（探测间隔，默认 60s）、timeout_ms（超时，默认 5000ms）。")
           .Produces<SiteResponse>(StatusCodes.Status201Created)
           .Produces(StatusCodes.Status400BadRequest);

        app.MapDelete("/api/v1/uptime/sites/{id:int}", async (MonitorService svc, int id) =>
           {
               var ok = await svc.DeleteSiteAsync(id);
               return ok ? Results.NoContent() : Results.NotFound();
           })
           .WithTags("站点监控")
           .WithSummary("删除监控站点")
           .WithDescription("移除一个监控站点及其所有历史检查记录。")
           .Produces(StatusCodes.Status204NoContent)
           .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/api/v1/uptime/sites/{id:int}/history", async (MonitorService svc, int id) =>
           {
               var history = await svc.GetSiteUptimeAsync(id);
               return history is null ? Results.NotFound() : Results.Ok(history);
           })
           .WithTags("站点监控")
           .WithSummary("站点可用性历史")
           .WithDescription("返回指定站点 24 小时内的可用性统计，包含可用率百分比、总检查次数、正常次数，以及每次检查的详细记录（状态码、响应时间）。")
           .Produces<UptimeHistoryResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        // ======== 应用健康检查 ========

        app.MapGet("/api/v1/health/services", async (MonitorService svc) =>
           {
               var records = await svc.GetLatestHealthRecordsAsync();
               return Results.Ok(records);
           })
           .WithTags("应用健康")
           .WithSummary("服务健康状态")
           .WithDescription("返回所有被监控服务的最近一次健康检查结果，含服务存活状态和响应延迟。\n\n与站点监控不同，此接口通过 Docker 内网地址调用各服务的深度健康端点（需 JWT）。\n数据由后台 Agent 每 30 秒采集一次。")
           .Produces<List<HealthRecordResponse>>(StatusCodes.Status200OK);

        // TODO: /api/v1/health/databases — 数据库深度指标（MySQL/Redis/MongoDB）

        // ======== 告警规则 ========

        app.MapGet("/api/v1/alerts/rules", async (MonitorService svc) =>
           {
               var rules = await svc.GetAlertRulesAsync();
               return Results.Ok(rules);
           })
           .WithTags("告警")
           .WithSummary("告警规则列表")
           .WithDescription("返回所有已配置的告警规则，每条规则包含监控指标、比较运算符、阈值和持续时长。")
           .Produces<List<AlertRuleResponse>>(StatusCodes.Status200OK);

        app.MapPost("/api/v1/alerts/rules", async (MonitorService svc, AlertRuleRequest req) =>
           {
               if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Metric))
                   return Results.BadRequest(new { error = "规则名称和监控指标不能为空" });
               var rule = await svc.CreateAlertRuleAsync(req);
               return Results.Created($"/api/v1/alerts/rules/{rule.Id}", rule);
           })
           .WithTags("告警")
           .WithSummary("创建告警规则")
           .WithDescription("新增一条告警规则。当监控指标连续超过阈值达到持续时长时触发告警。\n\n必填字段：name（规则名称）、metric（监控指标名）、operator（比较运算符）、threshold（阈值）。\n可选字段：duration_sec（持续时长，默认 300s）。")
           .Produces<AlertRuleResponse>(StatusCodes.Status201Created)
           .Produces(StatusCodes.Status400BadRequest);

        app.MapPut("/api/v1/alerts/rules/{id:int}", async (MonitorService svc, int id, AlertRuleRequest req) =>
           {
               var rule = await svc.UpdateAlertRuleAsync(id, req);
               return rule is null ? Results.NotFound() : Results.Ok(rule);
           })
           .WithTags("告警")
           .WithSummary("修改告警规则")
           .WithDescription("更新指定告警规则的完整配置（全量替换）。规则不存在返回 404。")
           .Produces<AlertRuleResponse>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        app.MapDelete("/api/v1/alerts/rules/{id:int}", async (MonitorService svc, int id) =>
           {
               var ok = await svc.DeleteAlertRuleAsync(id);
               return ok ? Results.NoContent() : Results.NotFound();
           })
           .WithTags("告警")
           .WithSummary("删除告警规则")
           .WithDescription("移除一条告警规则及其关联的所有告警事件。")
           .Produces(StatusCodes.Status204NoContent)
           .Produces(StatusCodes.Status404NotFound);

        // ======== 告警事件 ========

        app.MapGet("/api/v1/alerts/history", async (MonitorService svc, int? limit) =>
           {
               var events = await svc.GetAlertEventsAsync(limit ?? 50);
               return Results.Ok(events);
           })
           .WithTags("告警")
           .WithSummary("告警事件历史")
           .WithDescription("返回最近触发的告警事件列表，含触发时间、恢复时间、告警消息和严重级别。\n\n参数 limit 控制返回条数（默认 50，最大 200）。结果按触发时间倒序排列。")
           .Produces<List<AlertEventResponse>>(StatusCodes.Status200OK);

        // ======== TODO: 后续模块 ========

        // Nginx 监控 — 需 Agent 采集 nginx stub_status
        // 通知渠道配置
        // CI/CD 流水线 — 需 GitHub REST API 集成
    }
}
