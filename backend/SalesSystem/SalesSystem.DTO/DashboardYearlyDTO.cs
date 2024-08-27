using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DTO
{
    public class DashboardYearlyDTO
    {
        public int TotalSales { get; set; }
        public string TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public List<YearlySalesDTO> YearlySales { get; set; }
        public List<TopProductsDTO> topProducts { get; set; }
    }
}
