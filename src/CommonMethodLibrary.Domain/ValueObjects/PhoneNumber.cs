using CommonMethodLibrary.Domain.Common;
using System.Text.RegularExpressions;

namespace CommonMethodLibrary.Domain.ValueObjects;

/// <summary>
/// 手机号值对象 - 封装手机号及其验证逻辑
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 创建手机号值对象（中国大陆）
    /// </summary>
    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("手机号不能为空", nameof(phoneNumber));

        if (!IsValid(phoneNumber))
            throw new ArgumentException("手机号格式不正确", nameof(phoneNumber));

        return new PhoneNumber(phoneNumber);
    }

    private static bool IsValid(string phoneNumber)
    {
        const string pattern = @"^1[3-9]\d{9}$";
        return Regex.IsMatch(phoneNumber, pattern);
    }

    /// <summary>
    /// 获取掩码后的手机号（用于显示）
    /// </summary>
    public string GetMasked()
    {
        if (Value.Length != 11)
            return Value;

        return $"{Value[..3]}****{Value[^4..]}";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
