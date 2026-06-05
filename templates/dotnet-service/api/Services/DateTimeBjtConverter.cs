using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonitorApi.Services;

/// <summary>全局 JSON 时间转换器 — 序列化时 UTC → 北京时间 (UTC+8)</summary>
public class DateTimeBjtConverter : JsonConverter<DateTime>
{
    private static readonly TimeSpan BjtOffset = TimeSpan.FromHours(8);

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var bjt = DateTime.SpecifyKind(value.Add(BjtOffset), DateTimeKind.Local);
        writer.WriteStringValue(bjt);
    }
}
