namespace CommonMethodLibrary.Core.Helpers;

/// <summary>
/// 日期时间处理工具类
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// 获取当前时间戳（秒）
    /// </summary>
    public static long GetCurrentTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// 获取当前时间戳（毫秒）
    /// </summary>
    public static long GetCurrentTimestampMilliseconds()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// 时间戳转DateTime
    /// </summary>
    public static DateTime TimestampToDateTime(long timestamp, bool isMilliseconds = false)
    {
        var offset = isMilliseconds
            ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
            : DateTimeOffset.FromUnixTimeSeconds(timestamp);

        return offset.LocalDateTime;
    }

    /// <summary>
    /// DateTime转时间戳
    /// </summary>
    public static long DateTimeToTimestamp(DateTime dateTime, bool toMilliseconds = false)
    {
        var offset = new DateTimeOffset(dateTime);
        return toMilliseconds
            ? offset.ToUnixTimeMilliseconds()
            : offset.ToUnixTimeSeconds();
    }

    /// <summary>
    /// 获取友好时间描述（如：刚刚、5分钟前、昨天等）
    /// </summary>
    public static string GetFriendlyTimeSpan(DateTime dateTime)
    {
        var span = DateTime.Now - dateTime;

        if (span.TotalSeconds < 60)
            return "刚刚";

        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes}分钟前";

        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours}小时前";

        if (span.TotalDays < 2)
            return "昨天";

        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays}天前";

        return dateTime.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 判断是否为工作日
    /// </summary>
    public static bool IsWorkday(DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }

    /// <summary>
    /// 获取本周的第一天（周一）
    /// </summary>
    public static DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    /// <summary>
    /// 获取本月的第一天
    /// </summary>
    public static DateTime GetStartOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// 获取本月的最后一天
    /// </summary>
    public static DateTime GetEndOfMonth(DateTime date)
    {
        return GetStartOfMonth(date).AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// 计算年龄
    /// </summary>
    public static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// 判断两个日期是否为同一天
    /// </summary>
    public static bool IsSameDay(DateTime date1, DateTime date2)
    {
        return date1.Date == date2.Date;
    }

    /// <summary>
    /// 格式化为ISO 8601格式
    /// </summary>
    public static string ToIso8601(DateTime dateTime)
    {
        return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
}
