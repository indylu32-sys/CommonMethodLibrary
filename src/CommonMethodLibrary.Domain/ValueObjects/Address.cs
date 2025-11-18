using CommonMethodLibrary.Domain.Common;

namespace CommonMethodLibrary.Domain.ValueObjects;

/// <summary>
/// 地址值对象 - 封装地址信息
/// </summary>
public sealed class Address : ValueObject
{
    public string Province { get; }
    public string City { get; }
    public string District { get; }
    public string Street { get; }
    public string? PostalCode { get; }

    private Address(string province, string city, string district, string street, string? postalCode)
    {
        Province = province;
        City = city;
        District = district;
        Street = street;
        PostalCode = postalCode;
    }

    /// <summary>
    /// 创建地址值对象
    /// </summary>
    public static Address Create(string province, string city, string district, string street, string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(province))
            throw new ArgumentException("省份不能为空", nameof(province));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("城市不能为空", nameof(city));

        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("区县不能为空", nameof(district));

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("街道地址不能为空", nameof(street));

        return new Address(province, city, district, street, postalCode);
    }

    /// <summary>
    /// 获取完整地址
    /// </summary>
    public string GetFullAddress()
    {
        return $"{Province}{City}{District}{Street}";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Province;
        yield return City;
        yield return District;
        yield return Street;
        yield return PostalCode;
    }

    public override string ToString() => GetFullAddress();
}
