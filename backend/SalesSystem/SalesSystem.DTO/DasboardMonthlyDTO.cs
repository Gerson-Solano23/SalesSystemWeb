using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.DTO
{
    public class DasboardMonthlyDTO
    { 
        public int TotalSales { get; set; }
        public string TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public List<MonthlySalesDTO> MonthlySales { get; set; }

        public List<TopProductsDTO> topProducts { get; set; }
    }
}

