using System.Text.RegularExpressions;

namespace CommonMethodLibrary.Core.Helpers;

/// <summary>
/// 验证工具类
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// 验证邮箱格式
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 验证手机号（中国大陆）
    /// </summary>
    public static bool IsValidPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        const string pattern = @"^1[3-9]\d{9}$";
        return Regex.IsMatch(phone, pattern);
    }

    /// <summary>
    /// 验证身份证号（中国大陆）
    /// </summary>
    public static bool IsValidIdCard(string idCard)
    {
        if (string.IsNullOrWhiteSpace(idCard))
            return false;

        // 支持15位和18位身份证号
        const string pattern = @"^[1-9]\d{5}(18|19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$";
        return Regex.IsMatch(idCard, pattern);
    }

    /// <summary>
    /// 验证URL格式
    /// </summary>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// 验证IP地址
    /// </summary>
    public static bool IsValidIPAddress(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        const string pattern = @"^((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)$";
        return Regex.IsMatch(ip, pattern);
    }

    /// <summary>
    /// 验证强密码（至少8位，包含大小写字母、数字和特殊字符）
    /// </summary>
    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"\d");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    /// <summary>
    /// 验证是否为数字
    /// </summary>
    public static bool IsNumeric(string str)
    {
        return double.TryParse(str, out _);
    }

    /// <summary>
    /// 验证是否为整数
    /// </summary>
    public static bool IsInteger(string str)
    {
        return int.TryParse(str, out _);
    }

    /// <summary>
    /// 验证字符串长度
    /// </summary>
    public static bool IsValidLength(string str, int minLength, int maxLength)
    {
        if (string.IsNullOrEmpty(str))
            return minLength == 0;

        return str.Length >= minLength && str.Length <= maxLength;
    }

    /// <summary>
    /// 验证是否只包含字母
    /// </summary>
    public static bool IsAlpha(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        return Regex.IsMatch(str, @"^[a-zA-Z]+$");
    }

    /// <summary>
    /// 验证是否只包含字母和数字
    /// </summary>
    public static bool IsAlphaNumeric(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        return Regex.IsMatch(str, @"^[a-zA-Z0-9]+$");
    }

    /// <summary>
    /// 验证邮政编码（中国大陆）
    /// </summary>
    public static bool IsValidPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return false;

        const string pattern = @"^\d{6}$";
        return Regex.IsMatch(postalCode, pattern);
    }
}
