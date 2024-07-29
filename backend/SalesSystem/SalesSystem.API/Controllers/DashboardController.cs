using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
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
        [Route("monthSummary")]
        public async Task<IActionResult> MonthSummary()
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
    }
}
