using System.Net.Http.Json;
using MaxMind.GeoIP2;

namespace AuthApi.Services;

/// <summary>
/// IP 地理定位服务
/// <para>优先查询 MaxMind GeoLite2 离线数据库（<1ms），查不到时 fallback 到 ip-api.com。</para>
/// <para>数据库文件路径通过配置项 GeoIP:DatabasePath 指定，默认 /app/GeoIP/GeoLite2-City.mmdb。</para>
/// </summary>
public class IpGeoService : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _dbPath;
    private DatabaseReader? _reader;
    private readonly object _lock = new();

    public IpGeoService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _dbPath = config.GetValue<string>("GeoIP:DatabasePath") ?? "/app/GeoIP/GeoLite2-City.mmdb";
    }

    /// <summary>解析 IP 所在城市</summary>
    public async Task<string?> GetCityAsync(string ip)
    {
        if (string.IsNullOrEmpty(ip) || IsPrivateIp(ip))
            return null;

        // 1. 优先离线数据库
        var local = GetCityFromLocal(ip);
        if (local != null) return local;

        // 2. fallback 到在线 API
        return await GetCityFromApiAsync(ip);
    }

    private string? GetCityFromLocal(string ip)
    {
        try
        {
            var reader = GetReader();
            if (reader == null) return null;

            if (reader.TryCity(ip, out var response))
            {
                var parts = new[]
                {
                    response?.Country?.Names?.GetValueOrDefault("zh-CN"),
                    response?.MostSpecificSubdivision?.Names?.GetValueOrDefault("zh-CN"),
                    response?.City?.Names?.GetValueOrDefault("zh-CN")
                }.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

                return parts.Count > 0 ? string.Join("·", parts) : null;
            }
        }
        catch
        {
            // 数据库损坏或格式不兼容，走 fallback
        }
        return null;
    }

    private DatabaseReader? GetReader()
    {
        if (_reader != null) return _reader;

        lock (_lock)
        {
            if (_reader != null) return _reader;
            if (File.Exists(_dbPath))
            {
                _reader = new DatabaseReader(_dbPath);
                return _reader;
            }
        }
        return null;
    }

    private async Task<string?> GetCityFromApiAsync(string ip)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<IpApiResponse>(
                $"http://ip-api.com/json/{ip}?fields=country,regionName,city,status");

            if (result?.Status == "success")
            {
                var parts = new[] { result.Country, result.RegionName, result.City }
                    .Where(x => !string.IsNullOrEmpty(x)).Distinct();
                return string.Join("·", parts);
            }
        }
        catch { }
        return null;
    }

    private static bool IsPrivateIp(string ip)
    {
        if (ip is "unknown" or "127.0.0.1" or "::1") return true;
        if (ip.StartsWith("10.") || ip.StartsWith("192.168.") || ip.StartsWith("172.16.")) return true;
        return false;
    }

    public void Dispose() => _reader?.Dispose();

    private class IpApiResponse
    {
        public string? Status { get; set; }
        public string? Country { get; set; }
        public string? RegionName { get; set; }
        public string? City { get; set; }
    }
}
