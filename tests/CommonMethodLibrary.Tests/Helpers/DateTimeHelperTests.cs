using CommonMethodLibrary.Core.Helpers;
using Xunit;

namespace CommonMethodLibrary.Tests.Helpers;

/// <summary>
/// DateTimeHelper单元测试
/// </summary>
public class DateTimeHelperTests
{
    [Fact]
    public void GetCurrentTimestamp_ShouldReturnPositiveNumber()
    {
        // Act
        var timestamp = DateTimeHelper.GetCurrentTimestamp();

        // Assert
        Assert.True(timestamp > 0);
    }

    [Fact]
    public void TimestampToDateTime_DateTimeToTimestamp_ShouldRoundTrip()
    {
        // Arrange
        var originalTimestamp = 1609459200L; // 2021-01-01 00:00:00 UTC

        // Act
        var dateTime = DateTimeHelper.TimestampToDateTime(originalTimestamp);
        var resultTimestamp = DateTimeHelper.DateTimeToTimestamp(dateTime);

        // Assert
        Assert.Equal(originalTimestamp, resultTimestamp);
    }

    [Fact]
    public void CalculateAge_ShouldCalculateCorrectly()
    {
        // Arrange
        var birthDate = DateTime.Today.AddYears(-30);

        // Act
        var age = DateTimeHelper.CalculateAge(birthDate);

        // Assert
        Assert.Equal(30, age);
    }

    [Theory]
    [InlineData("2024-01-01", true)]  // Monday
    [InlineData("2024-01-06", false)] // Saturday
    [InlineData("2024-01-07", false)] // Sunday
    public void IsWorkday_ShouldIdentifyCorrectly(string dateString, bool expected)
    {
        // Arrange
        var date = DateTime.Parse(dateString);

        // Act
        var result = DateTimeHelper.IsWorkday(date);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetStartOfMonth_ShouldReturnFirstDay()
    {
        // Arrange
        var date = new DateTime(2024, 5, 15);

        // Act
        var result = DateTimeHelper.GetStartOfMonth(date);

        // Assert
        Assert.Equal(new DateTime(2024, 5, 1), result);
    }

    [Fact]
    public void GetEndOfMonth_ShouldReturnLastDay()
    {
        // Arrange
        var date = new DateTime(2024, 2, 15); // February

        // Act
        var result = DateTimeHelper.GetEndOfMonth(date);

        // Assert
        Assert.Equal(new DateTime(2024, 2, 29), result); // 2024 is leap year
    }
}
