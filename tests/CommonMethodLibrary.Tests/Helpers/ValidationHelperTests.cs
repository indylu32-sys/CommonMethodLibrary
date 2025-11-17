using CommonMethodLibrary.Core.Helpers;
using Xunit;

namespace CommonMethodLibrary.Tests.Helpers;

/// <summary>
/// ValidationHelper单元测试
/// </summary>
public class ValidationHelperTests
{
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@example.co.uk", true)]
    [InlineData("invalid.email", false)]
    [InlineData("@example.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_ShouldValidateCorrectly(string email, bool expected)
    {
        // Act
        var result = ValidationHelper.IsValidEmail(email);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("13800138000", true)]
    [InlineData("15912345678", true)]
    [InlineData("12345678901", false)]
    [InlineData("1380013800", false)]
    [InlineData("", false)]
    public void IsValidPhoneNumber_ShouldValidateCorrectly(string phone, bool expected)
    {
        // Act
        var result = ValidationHelper.IsValidPhoneNumber(phone);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://www.example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("www.example.com", false)]
    [InlineData("not a url", false)]
    public void IsValidUrl_ShouldValidateCorrectly(string url, bool expected)
    {
        // Act
        var result = ValidationHelper.IsValidUrl(url);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("192.168.1.1", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("256.1.1.1", false)]
    [InlineData("192.168.1", false)]
    [InlineData("not.an.ip", false)]
    public void IsValidIPAddress_ShouldValidateCorrectly(string ip, bool expected)
    {
        // Act
        var result = ValidationHelper.IsValidIPAddress(ip);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Password123!", true)]
    [InlineData("Abc12345!", true)]
    [InlineData("password", false)]
    [InlineData("PASSWORD", false)]
    [InlineData("Pass123", false)]
    [InlineData("", false)]
    public void IsStrongPassword_ShouldValidateCorrectly(string password, bool expected)
    {
        // Act
        var result = ValidationHelper.IsStrongPassword(password);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("123.45", true)]
    [InlineData("abc", false)]
    [InlineData("12.34.56", false)]
    public void IsNumeric_ShouldValidateCorrectly(string input, bool expected)
    {
        // Act
        var result = ValidationHelper.IsNumeric(input);

        // Assert
        Assert.Equal(expected, result);
    }
}
