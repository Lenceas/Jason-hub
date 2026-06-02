using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace AuthApi.Services;

/// <summary>
/// IP 地理定位服务
/// <para>通过 ip-api.com 免费接口查询，结果缓存 24 小时避免重复请求。</para>
/// <para>免费限制：45 次/分钟/来源 IP，缓存命中后几乎不消耗配额。</para>
/// </summary>
public class IpGeoService
{
    private readonly HttpClient _http;
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public IpGeoService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>解析 IP 所在城市</summary>
    public async Task<string?> GetCityAsync(string ip)
    {
        if (string.IsNullOrEmpty(ip) || IsPrivateIp(ip))
            return null;

        // 命中缓存 → 直接返回
        if (_cache.TryGetValue(ip, out var cached) && cached.ExpiresAt > DateTime.UtcNow)
            return cached.City;

        // 缓存过期或未命中 → 查在线 API
        try
        {
            var result = await _http.GetFromJsonAsync<IpApiResponse>(
                $"http://ip-api.com/json/{ip}?fields=country,regionName,city,status",
                CancellationToken.None);

            string? city = null;
            if (result?.Status == "success")
            {
                var parts = new[] { result.Country, result.RegionName, result.City }
                    .Where(x => !string.IsNullOrEmpty(x)).Distinct();
                city = string.Join("·", parts);
            }

            // 写入缓存（查不到也缓存，避免反复请求无效 IP）
            _cache[ip] = new CacheEntry(city, DateTime.UtcNow.Add(CacheTtl));
            return city;
        }
        catch
        {
            // 网络超时，返回 null（不缓存，下次重试）
            return null;
        }
    }

    private static bool IsPrivateIp(string ip)
    {
        if (ip is "unknown" or "127.0.0.1" or "::1") return true;
        if (ip.StartsWith("10.") || ip.StartsWith("192.168.") || ip.StartsWith("172.16.")) return true;
        return false;
    }

    private record CacheEntry(string? City, DateTime ExpiresAt);

    private class IpApiResponse
    {
        public string? Status { get; set; }
        public string? Country { get; set; }
        public string? RegionName { get; set; }
        public string? City { get; set; }
    }
}
