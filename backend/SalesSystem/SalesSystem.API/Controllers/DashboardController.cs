using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize]
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
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Summary()
        {
            var response = new Response<DashboardDTO>();

            try
            {
                response.status = true;
                response.data = await _dashBoardService.Summary();

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("MonthSummary")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> MonthSummary(int month, int? year)
        {
            int officialYear = 0;
            var response = new Response<DasboardMonthlyDTO>();
            if (year == null) {
                officialYear = DateTime.UtcNow.Year;
            }
            else
            {
                officialYear = year.Value;
            }
            try
            {
                response.status = true;
                response.data = await _dashBoardService.MonthlySummary(month, officialYear);

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }
        
        [HttpGet]
        [Route("YearSummary")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> YearSummary(int year)
        {
            int officialYear = 0;
            var response = new Response<DashboardYearlyDTO>();
         
            try
            {
                response.status = true;
                response.data = await _dashBoardService.YearSummary(year);

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("RangeDatesSummary")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RangeDatesSummary(string startDate, string endDate)
        {
            int officialYear = 0;
            var response = new Response<DashboardRangeDTO>();

            try
            {
                response.status = true;
                response.data = await _dashBoardService.RangeDatesSummary(startDate, endDate);

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }
    }
}
