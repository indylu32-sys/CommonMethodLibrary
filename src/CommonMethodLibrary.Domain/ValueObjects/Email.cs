using CommonMethodLibrary.Domain.Common;
using System.Text.RegularExpressions;

namespace CommonMethodLibrary.Domain.ValueObjects;

/// <summary>
/// 邮箱值对象 - 封装邮箱地址及其验证逻辑
/// </summary>
public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// 创建邮箱值对象
    /// </summary>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("邮箱不能为空", nameof(email));

        if (!IsValid(email))
            throw new ArgumentException("邮箱格式不正确", nameof(email));

        return new Email(email.ToLowerInvariant());
    }

    private static bool IsValid(string email)
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
