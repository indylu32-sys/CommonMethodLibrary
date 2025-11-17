using CommonMethodLibrary.Core.Helpers;
using Xunit;

namespace CommonMethodLibrary.Tests.Helpers;

/// <summary>
/// StringHelper单元测试
/// </summary>
public class StringHelperTests
{
    [Theory]
    [InlineData("HelloWorld", "helloWorld")]
    [InlineData("ABC", "aBC")]
    [InlineData("a", "a")]
    public void ToCamelCase_ShouldConvertCorrectly(string input, string expected)
    {
        // Act
        var result = StringHelper.ToCamelCase(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("helloWorld", "HelloWorld")]
    [InlineData("abc", "Abc")]
    [InlineData("A", "A")]
    public void ToPascalCase_ShouldConvertCorrectly(string input, string expected)
    {
        // Act
        var result = StringHelper.ToPascalCase(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("UserName", "user_name")]
    [InlineData("ABC", "abc")]
    public void ToSnakeCase_ShouldConvertCorrectly(string input, string expected)
    {
        // Act
        var result = StringHelper.ToSnakeCase(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Truncate_ShouldTruncateWithDefaultSuffix()
    {
        // Arrange
        var input = "This is a long string";

        // Act
        var result = StringHelper.Truncate(input, 10);

        // Assert
        Assert.Equal("This is...", result);
    }

    [Fact]
    public void GenerateRandomString_ShouldGenerateCorrectLength()
    {
        // Act
        var result = StringHelper.GenerateRandomString(15);

        // Assert
        Assert.Equal(15, result.Length);
    }

    [Theory]
    [InlineData("13800138000", 3, 4, "138****8000")]
    [InlineData("test@example.com", 4, 4, "test******.com")]
    public void MaskSensitiveInfo_ShouldMaskCorrectly(string input, int start, int end, string expected)
    {
        // Act
        var result = StringHelper.MaskSensitiveInfo(input, start, end);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToBase64_FromBase64_ShouldRoundTrip()
    {
        // Arrange
        var original = "Hello, World!";

        // Act
        var encoded = StringHelper.ToBase64(original);
        var decoded = StringHelper.FromBase64(encoded);

        // Assert
        Assert.Equal(original, decoded);
    }
}
