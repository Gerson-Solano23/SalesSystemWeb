using Microsoft.EntityFrameworkCore;
using SalesSystem.BLL.Forecasting;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.DTO;
using SalesSystem.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    /// <summary>
    /// Monthly revenue forecast. The model is refit from the database on every request
    /// (a few dozen monthly totals), so it always reflects the latest sales.
    /// </summary>
    public class DataPredictionService : IDataPrediction
    {
        public const int MaxMonthsAhead = 12;
        private const int MaxHistoryMonths = 36;

        private readonly DBSalesContext _dbContext;

        public DataPredictionService(DBSalesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SalesForecastDTO> ForecastMonthlyRevenue(int months)
        {
            if (months < 1 || months > MaxMonthsAhead)
            {
                throw new BusinessException($"Months must be between 1 and {MaxMonthsAhead}");
            }

            DateTime currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime historyStart = currentMonth.AddMonths(-MaxHistoryMonths);

            var totals = await _dbContext.Sales.AsNoTracking()
                .Where(s => s.DateRegistry >= historyStart && s.DateRegistry < currentMonth.AddMonths(1))
                .GroupBy(s => new { s.DateRegistry!.Value.Year, s.DateRegistry.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(s => s.Total ?? 0), Sales = g.Count() })
                .ToListAsync();

            var byMonth = totals.ToDictionary(t => new DateTime(t.Year, t.Month, 1), t => (t.Revenue, t.Sales));

            var result = new SalesForecastDTO
            {
                CurrentMonth = Point(currentMonth, byMonth)
            };

            // Complete months from the first month with sales up to last month; months without sales count as 0.
            var completeMonths = byMonth.Keys.Where(m => m < currentMonth).ToList();
            if (completeMonths.Count > 0)
            {
                for (DateTime month = completeMonths.Min(); month < currentMonth; month = month.AddMonths(1))
                {
                    result.History.Add(Point(month, byMonth));
                }
            }

            if (result.History.Count < HoltForecaster.MinimumPoints)
            {
                result.HasEnoughData = false;
                result.Message = $"At least {HoltForecaster.MinimumPoints} complete months of sales are needed to forecast " +
                                 $"(available: {result.History.Count}).";
                return result;
            }

            var forecast = HoltForecaster.Forecast(result.History.Select(p => (double)p.Revenue).ToList(), months);

            result.HasEnoughData = true;
            result.Forecast = forecast.Forecast.Select((f, i) => new RevenuePointDTO
            {
                Month = MonthKey(currentMonth.AddMonths(i)),
                Revenue = Round(f.Value),
                Lower = Round(f.Lower),
                Upper = Round(f.Upper)
            }).ToList();

            return result;
        }

        private static RevenuePointDTO Point(DateTime month, Dictionary<DateTime, (decimal Revenue, int Sales)> byMonth)
        {
            byMonth.TryGetValue(month, out var value);
            return new RevenuePointDTO { Month = MonthKey(month), Revenue = value.Revenue, Sales = value.Sales };
        }

        private static string MonthKey(DateTime month) => month.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        private static decimal Round(double value) => Math.Round((decimal)value, 2);
    }
}
