using SalesSystem.BLL.Forecasting;
using SalesSystem.BLL.Services;
using SalesSystem.Entity;
using SalesSystem.Utility;

namespace SalesSystem.Tests;

public class HoltForecasterTests
{
    [Fact]
    public void Forecast_LinearSeries_ContinuesTheTrend()
    {
        var result = HoltForecaster.Forecast(new double[] { 100, 200, 300, 400, 500 }, 2);

        Assert.Equal(600, result.Forecast[0].Value, 3);
        Assert.Equal(700, result.Forecast[1].Value, 3);
    }

    [Fact]
    public void Forecast_IntervalContainsValue_AndWidensWithHorizon()
    {
        var result = HoltForecaster.Forecast(new double[] { 120, 80, 150, 90, 160, 110, 170 }, 3);

        Assert.All(result.Forecast, f => Assert.InRange(f.Value, f.Lower, f.Upper));
        Assert.True(result.Forecast[2].Upper - result.Forecast[2].Lower > result.Forecast[0].Upper - result.Forecast[0].Lower);
    }

    [Fact]
    public void Forecast_NeverPredictsNegativeRevenue()
    {
        var result = HoltForecaster.Forecast(new double[] { 300, 200, 100, 10 }, 3);

        Assert.All(result.Forecast, f => Assert.True(f.Value >= 0 && f.Lower >= 0));
    }

    [Fact]
    public void Forecast_TooFewPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => HoltForecaster.Forecast(new double[] { 1, 2 }, 1));
    }
}

public class DataPredictionServiceTests : IDisposable
{
    private readonly SqliteDatabase _db = new();
    private static readonly DateTime CurrentMonth = new(DateTime.Now.Year, DateTime.Now.Month, 1);

    public void Dispose() => _db.Dispose();

    private void AddSale(DateTime date, decimal total) =>
        _db.Seed(ctx => ctx.Sales.Add(new Sale { numberDocument = Guid.NewGuid().ToString("N")[..4], PaymentType = "Cash", Total = total, DateRegistry = date }));

    [Fact]
    public async Task Forecast_UsesCompleteMonths_FillsGapsWithZero_AndStartsAtCurrentMonth()
    {
        AddSale(CurrentMonth.AddMonths(-4).AddDays(3), 100);
        AddSale(CurrentMonth.AddMonths(-4).AddDays(9), 50);
        // no sales three months ago
        AddSale(CurrentMonth.AddMonths(-2).AddDays(1), 200);
        AddSale(CurrentMonth.AddMonths(-1).AddDays(5), 300);
        AddSale(CurrentMonth.AddDays(1), 40);

        using var ctx = _db.NewContext();
        var result = await new DataPredictionService(ctx).ForecastMonthlyRevenue(2);

        Assert.True(result.HasEnoughData);
        Assert.Equal(new[] { 150m, 0m, 200m, 300m }, result.History.Select(h => h.Revenue));
        Assert.Equal(2, result.History[0].Sales);
        Assert.Equal(40m, result.CurrentMonth!.Revenue);
        Assert.Equal(new[] { CurrentMonth.ToString("yyyy-MM"), CurrentMonth.AddMonths(1).ToString("yyyy-MM") },
                     result.Forecast.Select(f => f.Month));
    }

    [Fact]
    public async Task Forecast_WithFewMonths_ReportsNotEnoughData()
    {
        AddSale(CurrentMonth.AddMonths(-1).AddDays(2), 100);

        using var ctx = _db.NewContext();
        var result = await new DataPredictionService(ctx).ForecastMonthlyRevenue(3);

        Assert.False(result.HasEnoughData);
        Assert.Empty(result.Forecast);
        Assert.NotNull(result.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task Forecast_InvalidMonths_Throws(int months)
    {
        using var ctx = _db.NewContext();
        await Assert.ThrowsAsync<BusinessException>(() => new DataPredictionService(ctx).ForecastMonthlyRevenue(months));
    }
}
