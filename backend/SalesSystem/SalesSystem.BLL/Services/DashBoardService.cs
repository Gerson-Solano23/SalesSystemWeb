using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DAL.DbSalesContext;
using SalesSystem.DAL.Repositories.Contract;
using SalesSystem.DTO;
using SalesSystem.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services
{
    public class DashBoardService : IDashBoard
    {
        private readonly ISalesRepository _salesRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IMapper _mapper;
        private readonly DBSalesContext _dbContext;
        public DashBoardService(ISalesRepository salesRepository, IGenericRepository<Product> productRepository, IMapper mapper, DBSalesContext dBSalesContext)
        {
            _salesRepository = salesRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _dbContext = dBSalesContext;
        }
        /// <summary>
        /// This method calculates the total sales for the last week and returns the result as a string.
        /// </summary>
        /// <param name="saleTable"></param>
        /// <param name="subtractNumberDays"></param>
        /// <returns></returns>

        private IQueryable<Sale> ReturnSales(IQueryable<Sale> saleTable, int subtractNumberDays)
        {
            DateTime? lastDate = saleTable.OrderByDescending(x => x.DateRegistry).Select(x => x.DateRegistry).First();

            lastDate = lastDate.Value.AddDays(subtractNumberDays);

            return saleTable.Where(x => x.DateRegistry >= lastDate.Value.Date);
        }
        #region dataLastWeek
        /// <summary>
        /// This method calculates the total sales for the last week and returns the result as a int.
        /// </summary>
        /// <returns></returns>
        private async Task<int> TotalSalesLastWeek()
        {
            int total = 0;
            IQueryable<Sale> _querySale = await _salesRepository.Consult();

            if (_querySale.Count() > 0)
            {
                var saleTable = ReturnSales(_querySale, -7);

                total = saleTable.Count();
            }

            return total;
        }
        /// <summary>
        /// This method calculates the total sales for the last month and returns the result as a string.
        /// </summary>
        /// <returns> </returns>
        private async Task<string> TotalRevenueLastWeek()
        {
            decimal result = 0;
            IQueryable<Sale> _querySale = await _salesRepository.Consult();

            if (_querySale.Count() > 0)
            {
                var saleTable = ReturnSales(_querySale, -7);

                result = saleTable.Select(x => x.Total).Sum(x => x.Value);
            }

            return Convert.ToString(result, new CultureInfo("es-CR"));
        }

        private async Task<int> TotalProducts()
        {
            int total = 0;
            IQueryable<Sale> _queryProduct = await _salesRepository.Consult();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var listTempDetails = _dbContext.Sales.Where(x => x.DateRegistry >= startDate && x.DateRegistry <= endDate).SelectMany(s => s.SaleDetails).ToList();
            //var list = listTempDetails.SelectMany();
            total = listTempDetails.Select(x => x.Quantity).Sum(x => x.Value);
            return total;
        }
        /// <summary>
        /// This method calculates sales for the last week and returns a dictionary with dates and total sales.
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string, int>> SalesLastWeek()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            IQueryable<Sale> _querySale = await _salesRepository.Consult();

            if (_querySale.Count() > 0)
            {
                // Filter sales for the last week using the ReturnSales method
                var saleTable = ReturnSales(_querySale, -7);
                // Group sales by date and count the total sales per day
                result = saleTable.GroupBy(x => x.DateRegistry.Value.Date).OrderBy(x => x.Key).Select(s => new { date = s.Key.ToString("dd/MM/yyyy"), total = s.Count() })
                    .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
            }

            return result;
        }
        public async Task<DashboardDTO> Summary()
        {
            DashboardDTO smDashboard = new DashboardDTO();

            try
            {
                smDashboard.TotalSales = await TotalSalesLastWeek();
                smDashboard.TotalRevenue = await TotalRevenueLastWeek();
                smDashboard.TotalProducts = await TotalProducts();
                smDashboard.topProducts = await topProductsLastWeek();
                List<WeeklySalesDTO> listSaleWeek = new List<WeeklySalesDTO>();

                foreach (KeyValuePair<string, int> item in await SalesLastWeek())
                {
                    listSaleWeek.Add(new WeeklySalesDTO { Date = item.Key, Total = item.Value });
                }

                smDashboard.SalesLastWeek = listSaleWeek;
            }
            catch (Exception)
            {

                throw;
            }

            return smDashboard;
        }
        public async Task<List<TopProductsDTO>> topProductsLastWeek()
        {
            List<TopProductsDTO> listTopProductsDTO = new List<TopProductsDTO>();

            try
            {
                listTopProductsDTO = await _dbContext.getTopTenProductsLastWeek();
            }
            catch (Exception)
            {

                throw;
            }
            return listTopProductsDTO;
        }
        #endregion dataLastWeek


        #region dataMonthly
        public async Task<DasboardMonthlyDTO> MonthlySummary(int month, int year)
        {
            DasboardMonthlyDTO sMonthlyDashboard = new DasboardMonthlyDTO();

            try
            {
                if (month == 0)
                {
                    month = DateTime.UtcNow.Month;
                }

                sMonthlyDashboard.TotalSales = await TotalSalesPerMonth(month, year);
                sMonthlyDashboard.TotalRevenue = await TotalRevenuePerMonth(month, year);
                sMonthlyDashboard.TotalProducts = await TotalProductsPerMonth(month, year);

                sMonthlyDashboard.MonthlySales = await SalesPerMonth(month, year);
                sMonthlyDashboard.topProducts = await topProductsPerMonth(month, year);
            }
            catch (Exception)
            {

                throw;
            }

            return sMonthlyDashboard;
        }
        public async Task<int> TotalSalesPerMonth(int month, int year)
        {
            int total = 0;
            IQueryable<Sale> _querySale = await _salesRepository.Consult();

            if (_querySale.Count() > 0)
            {
                total = _querySale.Where(x => x.DateRegistry.Value.Month == month && x.DateRegistry.Value.Year == year).Count();
            }

            return total;
        }

        public async Task<string> TotalRevenuePerMonth(int month, int year)
        {
            decimal result = 0;
            var sales = await _salesRepository.Consult();

            if (sales.Count() > 0)
            {
                result = sales.Where(x => x.DateRegistry.Value.Month == month && x.DateRegistry.Value.Year == year).Select(x => x.Total).Sum(x => x.Value);
            }

            return Convert.ToString(result, new CultureInfo("es-CR"));
        }

        private async Task<int> TotalProductsPerMonth(int month, int year)
        {
            int total = 0;
            IQueryable<Sale> _queryProduct = await _salesRepository.Consult();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var listTempDetails = _dbContext.Sales.Where(x => x.DateRegistry.Value.Month == month && x.DateRegistry.Value.Year == year).SelectMany(s => s.SaleDetails).ToList();
            //var list = listTempDetails.SelectMany();
            total = listTempDetails.Select(x => x.Quantity).Sum(x => x.Value);
            return total;
        }

        private async Task<List<MonthlySalesDTO>> SalesPerMonth(int month, int year)
        {
            List<MonthlySalesDTO> listMonthlySalesDTO = new List<MonthlySalesDTO>();
            List<Sale> salesList = new List<Sale>();

            try
            {
                // Obtener todas las ventas del mes y año especificados
                salesList = await _dbContext.Sales
                    .Where(x => x.DateRegistry.Value.Month == month && x.DateRegistry.Value.Year == year)
                    .ToListAsync();

                // Obtener el primer y último día del mes especificado
                DateTime firstDayOfMonth = new DateTime(year, month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Generar todas las semanas del mes
                var allWeeksInMonth = new Dictionary<int, int>();
                for (DateTime date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
                {
                    int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        date,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday);

                    if (!allWeeksInMonth.ContainsKey(weekNumber))
                    {
                        allWeeksInMonth[weekNumber] = 0;
                    }
                }

                // Agrupar las ventas por semana
                var groupedSales = salesList
                    .GroupBy(x => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        x.DateRegistry.Value,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday))
                    .ToList();

                // Fusionar las semanas generadas con las ventas agrupadas
                foreach (var group in groupedSales)
                {
                    int weekNumber = group.Key;
                    int totalSales = group.Count();

                    if (allWeeksInMonth.ContainsKey(weekNumber))
                    {
                        allWeeksInMonth[weekNumber] = totalSales;
                    }
                }

                // Crear la lista de DTOs en orden
                foreach (var week in allWeeksInMonth.OrderBy(x => x.Key))
                {
                    listMonthlySalesDTO.Add(new MonthlySalesDTO
                    {
                        Week = $"Week {week.Key}",
                        Total = week.Value
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }

            return listMonthlySalesDTO;
        }
        public async Task<List<TopProductsDTO>> topProductsPerMonth(int month, int year)
        {
            List<TopProductsDTO> listTopProductsDTO = new List<TopProductsDTO>();
            try
            {
                listTopProductsDTO = await _dbContext.getTopTenProductsMonthly(month, year);
            }
            catch (Exception)
            {

                throw;
            }
            return listTopProductsDTO;
        }
        #endregion dataMonthly

        #region dataYearly
        public async Task<List<TopProductsDTO>> topProductsPerYear(int year)
        {
            List<TopProductsDTO> listTopProductsDTO = new List<TopProductsDTO>();
            try
            {
                listTopProductsDTO = await _dbContext.getTopTenProductsYearly(year);
            }
            catch (Exception)
            {

                throw;
            }
            return listTopProductsDTO;
        }

        public async Task<DashboardYearlyDTO> YearSummary(int year)
        {
            DashboardYearlyDTO sYearlyDashboard = new DashboardYearlyDTO();

            try
            {

                sYearlyDashboard.TotalSales = await TotalSalesPerYear(year);
                sYearlyDashboard.TotalRevenue = await TotalRevenuePerYear(year);
                sYearlyDashboard.TotalProducts = await TotalProductsPerYear(year);

                sYearlyDashboard.YearlySales = await SalesPerYear(year);
                sYearlyDashboard.topProducts = await topProductsPerYear(year);
            }
            catch (Exception)
            {

                throw;
            }

            return sYearlyDashboard;
        }

        public async Task<int> TotalSalesPerYear(int year)
        {
            int total = 0;
            IQueryable<Sale> _querySale = await _salesRepository.Consult();

            if (_querySale.Count() > 0)
            {
                total = _querySale.Where(x => x.DateRegistry.Value.Year == year).Count();
            }

            return total;
        }

        public async Task<string> TotalRevenuePerYear(int year)
        {
            decimal result = 0;
            var sales = await _salesRepository.Consult();

            if (sales.Count() > 0)
            {
                result = sales.Where(x => x.DateRegistry.Value.Year == year).Select(x => x.Total).Sum(x => x.Value);
            }

            return Convert.ToString(result, new CultureInfo("en-US"));
        }
        private async Task<int> TotalProductsPerYear(int year)
        {
            int total = 0;
            IQueryable<Sale> _queryProduct = await _salesRepository.Consult();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var listTempDetails = _dbContext.Sales.Where(x => x.DateRegistry.Value.Year == year).SelectMany(s => s.SaleDetails).ToList();
            //var list = listTempDetails.SelectMany();
            total = listTempDetails.Select(x => x.Quantity).Sum(x => x.Value);
            return total;
        }

        private async Task<List<YearlySalesDTO>> SalesPerYear(int year)
        {
            List<YearlySalesDTO> listYearlySalesDTO = new List<YearlySalesDTO>();
            List<Sale> salesList = new List<Sale>();

            try
            {
                // Obtener todas las ventas del año especificado
                salesList = await _dbContext.Sales
                    .Where(x => x.DateRegistry.Value.Year == year)
                    .ToListAsync();

                // Generar todos los meses del año
                var allMonthsInYear = new Dictionary<string, int>();
                for (int month = 1; month <= 12; month++)
                {
                    string monthName = new DateTime(year, month, 1).ToString("MMMM", new CultureInfo("en-US"));
                    allMonthsInYear[monthName] = 0;
                }

                // Agrupar las ventas por mes
                var groupedSales = salesList
                    .GroupBy(x => x.DateRegistry.Value.Month)
                    .ToList();

                // Fusionar los meses generados con las ventas agrupadas
                foreach (var group in groupedSales)
                {
                    int month = group.Key;
                    int totalSales = group.Count();
                    string monthName = new DateTime(year, month, 1).ToString("MMMM", new CultureInfo("en-US"));

                    if (allMonthsInYear.ContainsKey(monthName))
                    {
                        allMonthsInYear[monthName] = totalSales;
                    }
                }

                // Crear la lista de DTOs en orden
                foreach (var month in allMonthsInYear.OrderBy(x => DateTime.ParseExact(x.Key, "MMMM", new CultureInfo("en-US")).Month))
                {
                    listYearlySalesDTO.Add(new YearlySalesDTO
                    {
                        Month = month.Key,
                        Total = month.Value
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }

            return listYearlySalesDTO;
        }
        #endregion dataYearly


        #region dataRangeDates
        public async Task<List<TopProductsDTO>> topProductsRangeDates(string startDate, string endDate)
        {
            List<TopProductsDTO> listTopProductsDTO = new List<TopProductsDTO>();
            try
            {
                listTopProductsDTO = await _dbContext.getTopTenProductsRangeDates(startDate, endDate);
            }
            catch (Exception)
            {

                throw;
            }
            return listTopProductsDTO;
        }

        public async Task<string> TotalRevenueRangeDates(string startDate, string endDate)
        {
            decimal result = 0;
            var sales = await _salesRepository.Consult();

            if (sales.Count() > 0)
            {
                DateTime dateStart = DateTime.ParseExact(startDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                DateTime dateEnd = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("en-US"));

                result = sales.Where(x => x.DateRegistry.Value.Date >= dateStart && x.DateRegistry.Value.Date <= dateEnd).Select(x => x.Total).Sum(x => x.Value);
            }

            return Convert.ToString(result, new CultureInfo("en-US"));

        }

        public async Task<int> TotalSalesRangeDates(string startDate, string endDate)
        {
            int total = 0;
            IQueryable<Sale> _querySale = _salesRepository.Consult().Result;

            if (_querySale.Count() > 0)
            {
                DateTime dateStart = DateTime.ParseExact(startDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                DateTime dateEnd = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("en-US"));

                total = _querySale.Where(x => x.DateRegistry.Value.Date >= dateStart && x.DateRegistry.Value.Date <= dateEnd).Count();
            }

            return total;
        }

        public async Task<int> TotalProductsRangeDates(string startDate, string endDate)
        {
            int total = 0;
            IQueryable<Sale> _queryProduct = _salesRepository.Consult().Result;
            try
            {
                DateTime dateStart = DateTime.ParseExact(startDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                DateTime dateEnd = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                var listTempDetails = _dbContext.Sales.Where(x => x.DateRegistry.Value.Date >= dateStart && x.DateRegistry.Value.Date <= dateEnd).SelectMany(s => s.SaleDetails).ToList();
                //var list = listTempDetails.SelectMany();
                total = listTempDetails.Select(x => x.Quantity).Sum(x => x.Value);
            }
            catch (Exception)
            {

                throw;
            }
            return total;
        }
        private async Task<List<RangeDateDTO>> RangeSalesInYears(DateTime startDate, DateTime endDate)
        {
            List<RangeDateDTO> listRangeDateDTO = new List<RangeDateDTO>();
            List<Sale> salesList = new List<Sale>();

            try
            {
                // Obtener todas las ventas en el rango de fechas
                salesList = await _dbContext.Sales
                    .Where(x => x.DateRegistry.Value.Date >= startDate && x.DateRegistry.Value.Date <= endDate)
                    .ToListAsync();

                // Crear todos los años en el rango
                for (int year = startDate.Year; year <= endDate.Year; year++)
                {
                    listRangeDateDTO.Add(new RangeDateDTO
                    {
                        date = $"Year {year}",
                        total = 0
                    });
                }

                // Agrupar las ventas por año
                var groupedSales = salesList
                    .GroupBy(x => x.DateRegistry.Value.Year)
                    .ToList();

                // Actualizar los totales en los DTOs
                foreach (var group in groupedSales)
                {
                    var dto = listRangeDateDTO.FirstOrDefault(x => x.date == $"Year {group.Key}");
                    if (dto != null)
                    {
                        dto.total = group.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return listRangeDateDTO;
        }


        private async Task<List<RangeDateDTO>> RangeSalesInMonths(DateTime startDate, DateTime endDate)
        {
            List<RangeDateDTO> listRangeDateDTO = new List<RangeDateDTO>();
            List<Sale> salesList = new List<Sale>();

            try
            {
                // Obtener todas las ventas en el rango de fechas
                salesList = await _dbContext.Sales
                    .Where(x => x.DateRegistry.Value.Date >= startDate && x.DateRegistry.Value.Date <= endDate)
                    .ToListAsync();

                // Crear todos los meses en el rango
                for (DateTime date = startDate; date <= endDate; date = date.AddMonths(1))
                {
                    listRangeDateDTO.Add(new RangeDateDTO
                    {
                        date = $"Month {date.Month}-{date.Year}",
                        total = 0
                    });
                }

                // Agrupar las ventas por mes y año
                var groupedSales = salesList
                    .GroupBy(x => new { x.DateRegistry.Value.Year, x.DateRegistry.Value.Month })
                    .ToList();

                // Actualizar los totales en los DTOs
                foreach (var group in groupedSales)
                {
                    var dto = listRangeDateDTO.FirstOrDefault(x => x.date == $"Month {group.Key.Month}-{group.Key.Year}");
                    if (dto != null)
                    {
                        dto.total = group.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return listRangeDateDTO;
        }


        private async Task<List<RangeDateDTO>> RangeSalesInWeeks(DateTime startDate, DateTime endDate)
        {
            List<RangeDateDTO> listRangeDateDTO = new List<RangeDateDTO>();
            List<Sale> salesList = new List<Sale>();

            try
            {
                // Obtener todas las ventas en el rango de fechas
                salesList = await _dbContext.Sales
                    .Where(x => x.DateRegistry.Value.Date >= startDate && x.DateRegistry.Value.Date <= endDate)
                    .ToListAsync();

                // Crear todas las semanas en el rango
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(7))
                {
                    int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        date,
                        CalendarWeekRule.FirstFourDayWeek,
                        DayOfWeek.Monday);

                    listRangeDateDTO.Add(new RangeDateDTO
                    {
                        date = $"Week {weekNumber}-{date.Year}",
                        total = 0
                    });
                }

                // Agrupar las ventas por semana y año
                var groupedSales = salesList
                    .GroupBy(x => new {
                        WeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                            x.DateRegistry.Value,
                            CalendarWeekRule.FirstFourDayWeek,
                            DayOfWeek.Monday),
                        x.DateRegistry.Value.Year
                    })
                    .ToList();

                // Actualizar los totales en los DTOs
                foreach (var group in groupedSales)
                {
                    var dto = listRangeDateDTO.FirstOrDefault(x => x.date == $"Week {group.Key.WeekNumber}-{group.Key.Year}");
                    if (dto != null)
                    {
                        dto.total = group.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return listRangeDateDTO;
        }


        private async Task<List<RangeDateDTO>> RangeSalesInDays(DateTime startDate, DateTime endDate)
        {
            List<RangeDateDTO> listRangeDateDTO = new List<RangeDateDTO>();
            List<Sale> salesList = new List<Sale>();

            try
            {
                // Obtener todas las ventas en el rango de fechas
                salesList = await _dbContext.Sales
                    .Where(x => x.DateRegistry.Value.Date >= startDate && x.DateRegistry.Value.Date <= endDate)
                    .ToListAsync();

                // Crear todos los días en el rango
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    listRangeDateDTO.Add(new RangeDateDTO
                    {
                        date = date.ToString("dd/MM/yyyy"),
                        total = 0
                    });
                }

                // Agrupar las ventas por día
                var groupedSales = salesList
                    .GroupBy(x => x.DateRegistry.Value.Date)
                    .ToList();

                // Actualizar los totales en los DTOs
                foreach (var group in groupedSales)
                {
                    var dto = listRangeDateDTO.FirstOrDefault(x => x.date == group.Key.ToString("dd/MM/yyyy"));
                    if (dto != null)
                    {
                        dto.total = group.Count();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return listRangeDateDTO;
        }

        public async Task<DashboardRangeDTO> RangeDatesSummary(string startDate, string endDate)
        {
            DashboardRangeDTO rangeDTO = new DashboardRangeDTO();
            rangeDTO.topProducts = await topProductsRangeDates(startDate, endDate);
            rangeDTO.TotalRevenue = await TotalRevenueRangeDates(startDate, endDate);
            rangeDTO.TotalSales = await TotalSalesRangeDates(startDate, endDate);
            rangeDTO.TotalProducts = await TotalProductsRangeDates(startDate, endDate);

            try
            {
                DateTime dateStart = DateTime.ParseExact(startDate, "dd/MM/yyyy", new CultureInfo("en-US"));
                DateTime dateEnd = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("en-US"));

                int countYear = CalculateYearDifference(dateStart, dateEnd);
                if (countYear > 1)
                {
                    rangeDTO.RangeSales = await RangeSalesInYears(dateStart, dateEnd);
                }
                else
                {
                    int countMonth = CalculateMonthDifference(dateStart, dateEnd);
                    if (countMonth > 2)
                    {
                        rangeDTO.RangeSales = await RangeSalesInMonths(dateStart, dateEnd);
                    }
                    else
                    {
                        int countWeeks = CalculateWeekDifference(dateStart, dateEnd);

                        if (countWeeks > 3)
                        {
                            rangeDTO.RangeSales = await RangeSalesInWeeks(dateStart, dateEnd);
                        }
                        else
                        {
                            int countDays = CalculateDayDifference(dateStart, dateEnd);
                            if (countDays > 0)
                            {
                                rangeDTO.RangeSales = await RangeSalesInDays(dateStart, dateEnd);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar excepción (puedes agregar un log aquí si lo deseas)
                throw;
            }

            return rangeDTO;
        }

        public int CalculateYearDifference(DateTime date1, DateTime date2)
        {
            int yearDifference = date2.Year - date1.Year;

            if (date2.Month < date1.Month || (date2.Month == date1.Month && date2.Day < date1.Day))
            {
                yearDifference--;
            }

            return yearDifference;
        }
        public int CalculateMonthDifference(DateTime date1, DateTime date2)
        {
            int monthDifference = (date2.Year - date1.Year) * 12 + date2.Month - date1.Month;

            if (date2.Day < date1.Day)
            {
                monthDifference--;
            }

            return monthDifference;
        }

        public int CalculateWeekDifference(DateTime date1, DateTime date2)
        {
            // Asegurarse de que date2 es mayor que date1
            if (date2 < date1)
            {
                var temp = date2;
                date2 = date1;
                date1 = temp;
            }

            // Obtener la diferencia total de días y luego dividir por 7 para obtener semanas completas
            int totalDaysDifference = (date2 - date1).Days;
            int weekDifference = totalDaysDifference / 7;

            return weekDifference;
        }
        public int CalculateDayDifference(DateTime date1, DateTime date2)
        {
            // Asegurarse de que date2 es mayor que date1
            if (date2 < date1)
            {
                var temp = date2;
                date2 = date1;
                date1 = temp;
            }

            // Obtener la diferencia total de días
            int dayDifference = (date2 - date1).Days;

            return dayDifference;
        }
        #endregion dataRangeDates

    }
}
