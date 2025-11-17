using System.Text.Json;
using System.Text.Json.Serialization;

namespace CommonMethodLibrary.Core.Helpers;

/// <summary>
/// JSON处理工具类
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>
    /// 序列化对象为JSON字符串
    /// </summary>
    public static string Serialize<T>(T obj, bool pretty = false)
    {
        return JsonSerializer.Serialize(obj, pretty ? PrettyOptions : DefaultOptions);
    }

    /// <summary>
    /// 反序列化JSON字符串为对象
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// 尝试反序列化JSON字符串
    /// </summary>
    public static bool TryDeserialize<T>(string json, out T? result)
    {
        try
        {
            result = Deserialize<T>(json);
            return result != null;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// 深度克隆对象
    /// </summary>
    public static T? DeepClone<T>(T obj)
    {
        var json = Serialize(obj);
        return Deserialize<T>(json);
    }

    /// <summary>
    /// 验证JSON字符串格式
    /// </summary>
    public static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 格式化JSON字符串（美化）
    /// </summary>
    public static string? FormatJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, PrettyOptions);
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// 压缩JSON字符串（移除空白）
    /// </summary>
    public static string? MinifyJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, DefaultOptions);
        }
        catch
        {
            return json;
        }
    }
}
