using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize(Policy = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashBoard _dashBoardService;

        public DashboardController(IDashBoard dashBoardService)
        {
            _dashBoardService = dashBoardService;
        }

        [HttpGet]
        [Route("Summary")]
        public async Task<IActionResult> Summary()
        {
            var response = new Response<DashboardDTO>
            {
                status = true,
                data = await _dashBoardService.Summary()
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("MonthSummary")]
        public async Task<IActionResult> MonthSummary(int month, int? year)
        {
            var response = new Response<DasboardMonthlyDTO>
            {
                status = true,
                data = await _dashBoardService.MonthlySummary(month, year ?? DateTime.Now.Year)
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("YearSummary")]
        public async Task<IActionResult> YearSummary(int year)
        {
            var response = new Response<DashboardYearlyDTO>
            {
                status = true,
                data = await _dashBoardService.YearSummary(year)
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("RangeDatesSummary")]
        public async Task<IActionResult> RangeDatesSummary(string startDate, string endDate)
        {
            var response = new Response<DashboardRangeDTO>
            {
                status = true,
                data = await _dashBoardService.RangeDatesSummary(startDate, endDate)
            };
            return Ok(response);
        }
    }
}
