using System.Text.Json;
using StackExchange.Redis;

namespace MonitorApi.Services;

/// <summary>Redis 缓存服务 — 实时数据缓存（TTL 60s），历史数据走 MySQL</summary>
public class RedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private IDatabase Db => _redis.GetDatabase();

    /// <summary>从缓存读取 JSON 并反序列化</summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await Db.StringGetAsync(key);
            if (value.IsNullOrEmpty) return null;
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 读取失败 {Key}", key);
            return null;
        }
    }

    /// <summary>写入缓存（默认 TTL 60 秒）</summary>
    public async Task SetAsync<T>(string key, T value, int ttlSeconds = 60)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await Db.StringSetAsync(key, json, TimeSpan.FromSeconds(ttlSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 写入失败 {Key}", key);
        }
    }

    /// <summary>获取缓存的字符串值</summary>
    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var value = await Db.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 读取失败 {Key}", key);
            return null;
        }
    }
}
