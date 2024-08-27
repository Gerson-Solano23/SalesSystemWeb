using SalesSystem.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.BLL.Services.Contract
{
    public interface IDashBoard
    {
        Task<DashboardDTO> Summary();
        Task<DasboardMonthlyDTO> MonthlySummary(int month, int year);
        Task<DashboardYearlyDTO> YearSummary(int year);

        Task<DashboardRangeDTO> RangeDatesSummary(string startDate, string endDate);

    }
}
