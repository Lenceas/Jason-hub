namespace __ProjectName__Api.Services;

/// <summary>__ProjectName__ 业务服务</summary>
public class __ProjectName__Service
{
    private readonly ILogger<__ProjectName__Service> _logger;

    public __ProjectName__Service(ILogger<__ProjectName__Service> logger)
    {
        _logger = logger;
    }

    /// <summary>示例方法 — 获取问候语</summary>
    /// <param name="name">用户名，不能为空</param>
    /// <returns>问候语字符串</returns>
    /// <example>
    /// var greeting = await service.GreetAsync("张三");
    /// // → "你好，张三！"
    /// </example>
    public async Task<string> GreetAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("名称不能为空", nameof(name));

        await Task.CompletedTask;
        _logger.LogInformation("GreetAsync 被调用: {Name}", name);
        return $"你好，{name}！";
    }
}
