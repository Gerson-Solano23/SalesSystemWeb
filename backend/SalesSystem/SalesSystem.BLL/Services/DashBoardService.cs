using AutoMapper;
using SalesSystem.BLL.Services.Contract;
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

        public DashBoardService(ISalesRepository salesRepository, IGenericRepository<Product> productRepository, IMapper mapper)
        {
            _salesRepository = salesRepository;
            _productRepository = productRepository;
            _mapper = mapper;
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

                result = saleTable.Select(x=>x.Total).Sum(x=>x.Value);
            }

            return Convert.ToString(result, new CultureInfo("es-CR"));
        }

        private async Task<int> TotalProducts()
        {
            IQueryable<Product> _queryProduct = await _productRepository.Consult();

            int total = _queryProduct.Count();
            
            return total;
        }
        /// <summary>
        /// This method calculates sales for the last week and returns a dictionary with dates and total sales.
        /// </summary>
        /// <returns></returns>
        private async Task<Dictionary<string,int>> SalesLastWeek()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            IQueryable<Sale> _querySale = await _salesRepository.Consult();

            if (_querySale.Count() > 0)
            {
                // Filter sales for the last week using the ReturnSales method
                var saleTable = ReturnSales(_querySale, -7);
                // Group sales by date and count the total sales per day
                result = saleTable.GroupBy(x => x.DateRegistry.Value.Date).OrderBy(x => x.Key).Select(s => new { date = s.Key.ToString("dd/MM/yyyy"), total = s.Count() })
                    .ToDictionary(keySelector: r=> r.date, elementSelector: r=> r.total);
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
    }
}
