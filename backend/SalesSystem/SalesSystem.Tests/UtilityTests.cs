using SalesSystem.BLL.Services;
using SalesSystem.Utility;

namespace SalesSystem.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_IsVerifiable_AndSalted()
    {
        string first = PasswordHasher.Hash("my-password");
        string second = PasswordHasher.Hash("my-password");

        Assert.NotEqual(first, second);
        Assert.True(PasswordHasher.Verify("my-password", first, out bool needsRehash));
        Assert.False(needsRehash);
        Assert.False(PasswordHasher.Verify("other-password", first, out _));
    }

    [Fact]
    public void Verify_PlainTextValue_MatchesAndAsksForRehash()
    {
        Assert.True(PasswordHasher.Verify("legacy", "legacy", out bool needsRehash));
        Assert.True(needsRehash);
        Assert.False(PasswordHasher.Verify("other", "legacy", out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("PBKDF2$abc$not-base64$!!")]
    public void Verify_InvalidStoredValue_ReturnsFalse(string? stored)
    {
        Assert.False(PasswordHasher.Verify("x", stored, out _));
    }
}

public class DateParserTests
{
    [Theory]
    [InlineData("05/03/2024", 2024, 3, 5)]
    [InlineData("5/3/2024", 2024, 3, 5)]
    public void Parse_AcceptsDayMonthYear(string value, int year, int month, int day)
    {
        Assert.Equal(new DateTime(year, month, day), DateParser.Parse(value, "date"));
    }

    [Theory]
    [InlineData("2024-03-05")]
    [InlineData("31/02/2024")]
    [InlineData("")]
    [InlineData(null)]
    public void Parse_InvalidValue_ThrowsBusinessException(string? value)
    {
        Assert.Throws<BusinessException>(() => DateParser.Parse(value, "date"));
    }

    [Fact]
    public void ParseRange_StartAfterEnd_Throws()
    {
        Assert.Throws<BusinessException>(() => DateParser.ParseRange("10/03/2024", "01/03/2024"));
    }
}

public class WeeklyTaskServiceTests
{
    [Theory]
    [InlineData("2024-10-16", "2024-10-07", "2024-10-13", "Report_Week-2024-W41.xlsx")] // Wednesday
    [InlineData("2024-10-14", "2024-10-07", "2024-10-13", "Report_Week-2024-W41.xlsx")] // Monday
    [InlineData("2024-10-13", "2024-09-30", "2024-10-06", "Report_Week-2024-W40.xlsx")] // Sunday
    [InlineData("2021-01-05", "2020-12-28", "2021-01-03", "Report_Week-2020-W53.xlsx")] // ISO year boundary
    public void GetLastCompletedWeek_ReturnsPreviousMondayToSunday(string now, string start, string end, string fileName)
    {
        var week = WeeklyTaskService.GetLastCompletedWeek(DateTime.Parse(now));

        Assert.Equal(DateTime.Parse(start), week.Start);
        Assert.Equal(DateTime.Parse(end), week.End);
        Assert.Equal(fileName, week.FileName);
    }
}

public class DashBoardServiceTests
{
    [Theory]
    [InlineData("2024-01-01", "2024-01-10", 0, 0, 1)]
    [InlineData("2024-01-01", "2024-04-15", 0, 3, 15)]
    [InlineData("2022-01-01", "2024-02-01", 2, 25, 108)]
    public void Differences_AreComputedInWholeUnits(string a, string b, int years, int months, int weeks)
    {
        var start = DateTime.Parse(a);
        var end = DateTime.Parse(b);

        Assert.Equal(years, DashBoardService.CalculateYearDifference(start, end));
        Assert.Equal(months, DashBoardService.CalculateMonthDifference(start, end));
        Assert.Equal(weeks, DashBoardService.CalculateWeekDifference(start, end));
    }
}
