using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;

namespace AuthApi.Services;

/// <summary>
/// IP 地理定位服务
/// <para>使用 ip2region xdb 离线数据库，查询 IP 所在国家/省份/城市，微秒级响应。</para>
/// <para>数据库文件通过配置项 GeoIP:DatabasePath 指定，默认 /app/GeoIP/ip2region_v4.xdb。</para>
/// </summary>
public class Ip2RegionService : IDisposable
{
    private readonly ISearcher _searcher;

    public Ip2RegionService(IConfiguration config)
    {
        var dbPath = config.GetValue<string>("ip2region:DatabasePath")
            ?? Path.Combine(AppContext.BaseDirectory, "ip2region", "ip2region_v4.xdb");

        if (!File.Exists(dbPath))
            throw new FileNotFoundException($"ip2region 数据库文件未找到: {dbPath}。请确保已下载 ip2region_v4.xdb 到指定路径。");

        _searcher = new Searcher(CachePolicy.File, dbPath);
    }

    /// <summary>解析 IP 所在城市</summary>
    public string? GetCity(string ip)
    {
        if (string.IsNullOrEmpty(ip) || IsPrivateIp(ip))
            return null;

        try
        {
            // ip2region 返回格式: 国家|区域|省份|城市|ISP
            // 例: 中国|0|广东省|深圳市|电信
            var result = _searcher.Search(ip);
            if (string.IsNullOrEmpty(result) || result == "0|0|0|0|0")
                return null;

            var parts = result.Split('|');
            var country = parts[0] != "0" ? parts[0] : null;
            var province = parts.Length > 2 && parts[2] != "0" ? parts[2] : null;
            var city = parts.Length > 3 && parts[3] != "0" ? parts[3] : null;

            var items = new[] { country, province, city }
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct();
            return string.Join("·", items);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>异步版本（兼容旧调用方）</summary>
    public Task<string?> GetCityAsync(string ip)
        => Task.FromResult(GetCity(ip));

    private static bool IsPrivateIp(string ip)
    {
        if (ip is "unknown" or "127.0.0.1" or "::1") return true;
        if (ip.StartsWith("10.") || ip.StartsWith("192.168.") || ip.StartsWith("172.16.")) return true;
        return false;
    }

    public void Dispose() => _searcher?.Dispose();
}
