using System.Text;
using System.Text.RegularExpressions;

namespace CommonMethodLibrary.Core.Helpers;

/// <summary>
/// 字符串处理工具类
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// 判断字符串是否为空或null
    /// </summary>
    public static bool IsNullOrEmpty(string? str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// 判断字符串是否为空、null或仅包含空白字符
    /// </summary>
    public static bool IsNullOrWhiteSpace(string? str) => string.IsNullOrWhiteSpace(str);

    /// <summary>
    /// 转换为驼峰命名
    /// </summary>
    public static string ToCamelCase(string str)
    {
        if (IsNullOrEmpty(str)) return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// 转换为帕斯卡命名
    /// </summary>
    public static string ToPascalCase(string str)
    {
        if (IsNullOrEmpty(str)) return str;
        return char.ToUpperInvariant(str[0]) + str[1..];
    }

    /// <summary>
    /// 转换为蛇形命名
    /// </summary>
    public static string ToSnakeCase(string str)
    {
        if (IsNullOrEmpty(str)) return str;
        return Regex.Replace(str, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
    }

    /// <summary>
    /// 截取字符串，超出部分用省略号替代
    /// </summary>
    public static string Truncate(string str, int maxLength, string suffix = "...")
    {
        if (IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;

        return str[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// 移除HTML标签
    /// </summary>
    public static string RemoveHtmlTags(string html)
    {
        if (IsNullOrEmpty(html)) return html;
        return Regex.Replace(html, "<.*?>", string.Empty);
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    public static string GenerateRandomString(int length, bool includeNumbers = true, bool includeSpecialChars = false)
    {
        const string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var chars = letters;
        if (includeNumbers) chars += numbers;
        if (includeSpecialChars) chars += specialChars;

        var random = new Random();
        var result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    /// <summary>
    /// 掩码敏感信息（如手机号、邮箱等）
    /// </summary>
    public static string MaskSensitiveInfo(string str, int visibleStart = 3, int visibleEnd = 4, char maskChar = '*')
    {
        if (IsNullOrEmpty(str) || str.Length <= visibleStart + visibleEnd)
            return str;

        var start = str[..visibleStart];
        var end = str[^visibleEnd..];
        var maskLength = str.Length - visibleStart - visibleEnd;

        return $"{start}{new string(maskChar, maskLength)}{end}";
    }

    /// <summary>
    /// 转换为Base64编码
    /// </summary>
    public static string ToBase64(string str, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(str);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// 从Base64解码
    /// </summary>
    public static string FromBase64(string base64, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = Convert.FromBase64String(base64);
        return encoding.GetString(bytes);
    }
}
