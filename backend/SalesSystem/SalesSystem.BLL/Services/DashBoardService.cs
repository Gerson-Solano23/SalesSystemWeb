using Microsoft.EntityFrameworkCore;
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
    /// Dashboard figures. Totals are aggregated by the database (COUNT/SUM) and the charts are built
    /// from per-day sale counts, so no sale rows are loaded into memory. Every period is a
    /// half-open interval [start, endExclusive) in server local time.
    /// </summary>
    public class DashBoardService : IDashBoard
    {
        private static readonly CultureInfo LabelCulture = CultureInfo.GetCultureInfo("en-US");

        private readonly DBSalesContext _dbContext;

        public DashBoardService(DBSalesContext dBSalesContext)
        {
            _dbContext = dBSalesContext;
        }

        private record Totals(int Sales, string Revenue, int Products);

        private async Task<Totals> GetTotals(DateTime start, DateTime endExclusive)
        {
            var sales = _dbContext.Sales.AsNoTracking()
                .Where(s => s.DateRegistry >= start && s.DateRegistry < endExclusive);

            int salesCount = await sales.CountAsync();
            decimal revenue = await sales.SumAsync(s => s.Total ?? 0);
            int products = await sales.SelectMany(s => s.SaleDetails).SumAsync(d => d.Quantity ?? 0);

            return new Totals(salesCount, FormatMoney(revenue), products);
        }

        private async Task<Dictionary<DateTime, int>> GetSalesPerDay(DateTime start, DateTime endExclusive)
        {
            return await _dbContext.Sales.AsNoTracking()
                .Where(s => s.DateRegistry >= start && s.DateRegistry < endExclusive)
                .GroupBy(s => s.DateRegistry!.Value.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Day, x => x.Count);
        }

        /// <summary>
        /// Groups the per-day counts of [start, end] into consecutive buckets named by <paramref name="label"/>,
        /// keeping empty buckets so charts show every period.
        /// </summary>
        private static List<(string Label, int Total)> Bucket(DateTime start, DateTime end, Dictionary<DateTime, int> perDay, Func<DateTime, string> label)
        {
            var result = new List<(string Label, int Total)>();
            for (DateTime day = start.Date; day <= end.Date; day = day.AddDays(1))
            {
                string key = label(day);
                int count = perDay.TryGetValue(day, out int c) ? c : 0;
                if (result.Count > 0 && result[^1].Label == key)
                {
                    result[^1] = (key, result[^1].Total + count);
                }
                else
                {
                    result.Add((key, count));
                }
            }
            return result;
        }

        private static string FormatMoney(decimal value) => value.ToString("N2", LabelCulture);
        private static string DayLabel(DateTime d) => d.ToString(DateParser.Format, CultureInfo.InvariantCulture);
        private static string WeekLabel(DateTime d) => $"Week {ISOWeek.GetWeekOfYear(d)}";
        private static string WeekYearLabel(DateTime d) => $"Week {ISOWeek.GetWeekOfYear(d)}-{ISOWeek.GetYear(d)}";
        private static string MonthName(DateTime d) => d.ToString("MMMM", LabelCulture);
        private static string MonthYearLabel(DateTime d) => $"Month {d.Month}-{d.Year}";
        private static string YearLabel(DateTime d) => $"Year {d.Year}";

        #region dataLastWeek
        /// <summary>Last 7 days, today included.</summary>
        public async Task<DashboardDTO> Summary()
        {
            DateTime end = DateTime.Now.Date;
            DateTime start = end.AddDays(-6);
            DateTime endExclusive = end.AddDays(1);

            var totals = await GetTotals(start, endExclusive);
            var perDay = await GetSalesPerDay(start, endExclusive);

            return new DashboardDTO
            {
                TotalSales = totals.Sales,
                TotalRevenue = totals.Revenue,
                TotalProducts = totals.Products,
                topProducts = await _dbContext.getTopTenProductsLastWeek(),
                SalesLastWeek = Bucket(start, end, perDay, DayLabel)
                    .Select(b => new WeeklySalesDTO { Date = b.Label, Total = b.Total })
                    .ToList()
            };
        }
        #endregion dataLastWeek

        #region dataMonthly
        public async Task<DasboardMonthlyDTO> MonthlySummary(int month, int year)
        {
            if (month == 0)
            {
                month = DateTime.Now.Month;
            }
            if (month < 1 || month > 12 || year < 1 || year > 9999)
            {
                throw new BusinessException("Invalid month or year");
            }

            DateTime start = new DateTime(year, month, 1);
            DateTime endExclusive = start.AddMonths(1);

            var totals = await GetTotals(start, endExclusive);
            var perDay = await GetSalesPerDay(start, endExclusive);

            return new DasboardMonthlyDTO
            {
                TotalSales = totals.Sales,
                TotalRevenue = totals.Revenue,
                TotalProducts = totals.Products,
                topProducts = await _dbContext.getTopTenProductsMonthly(month, year),
                MonthlySales = Bucket(start, endExclusive.AddDays(-1), perDay, WeekLabel)
                    .Select(b => new MonthlySalesDTO { Week = b.Label, Total = b.Total })
                    .ToList()
            };
        }
        #endregion dataMonthly

        #region dataYearly
        public async Task<DashboardYearlyDTO> YearSummary(int year)
        {
            if (year < 1 || year > 9999)
            {
                throw new BusinessException("Invalid year");
            }

            DateTime start = new DateTime(year, 1, 1);
            DateTime endExclusive = start.AddYears(1);

            var totals = await GetTotals(start, endExclusive);
            var perDay = await GetSalesPerDay(start, endExclusive);

            return new DashboardYearlyDTO
            {
                TotalSales = totals.Sales,
                TotalRevenue = totals.Revenue,
                TotalProducts = totals.Products,
                topProducts = await _dbContext.getTopTenProductsYearly(year),
                YearlySales = Bucket(start, endExclusive.AddDays(-1), perDay, MonthName)
                    .Select(b => new YearlySalesDTO { Month = b.Label, Total = b.Total })
                    .ToList()
            };
        }
        #endregion dataYearly

        #region dataRangeDates
        /// <summary>
        /// Summary for [startDate, endDate]. The chart granularity depends on the range length:
        /// years (over a year), months (over two months), weeks (over three weeks) or days.
        /// </summary>
        public async Task<DashboardRangeDTO> RangeDatesSummary(string startDate, string endDate)
        {
            var (start, end) = DateParser.ParseRange(startDate, endDate);
            DateTime endExclusive = end.AddDays(1);

            var totals = await GetTotals(start, endExclusive);
            var perDay = await GetSalesPerDay(start, endExclusive);

            Func<DateTime, string> label =
                CalculateYearDifference(start, end) > 1 ? YearLabel
                : CalculateMonthDifference(start, end) > 2 ? MonthYearLabel
                : CalculateWeekDifference(start, end) > 3 ? WeekYearLabel
                : DayLabel;

            return new DashboardRangeDTO
            {
                TotalSales = totals.Sales,
                TotalRevenue = totals.Revenue,
                TotalProducts = totals.Products,
                topProducts = await _dbContext.getTopTenProductsRangeDates(DayLabel(start), DayLabel(end)),
                RangeSales = Bucket(start, end, perDay, label)
                    .Select(b => new RangeDateDTO { date = b.Label, total = b.Total })
                    .ToList()
            };
        }

        public static int CalculateYearDifference(DateTime date1, DateTime date2)
        {
            int yearDifference = date2.Year - date1.Year;
            if (date2.Month < date1.Month || (date2.Month == date1.Month && date2.Day < date1.Day))
            {
                yearDifference--;
            }
            return yearDifference;
        }

        public static int CalculateMonthDifference(DateTime date1, DateTime date2)
        {
            int monthDifference = (date2.Year - date1.Year) * 12 + date2.Month - date1.Month;
            if (date2.Day < date1.Day)
            {
                monthDifference--;
            }
            return monthDifference;
        }

        public static int CalculateWeekDifference(DateTime date1, DateTime date2)
        {
            return Math.Abs((date2 - date1).Days) / 7;
        }
        #endregion dataRangeDates
    }
}
