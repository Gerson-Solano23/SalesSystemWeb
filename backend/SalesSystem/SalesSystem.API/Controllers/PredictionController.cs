using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize(Policy = "Admin_Supervisor")]
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly IDataPrediction _dataPrediction;

        public PredictionController(IDataPrediction dataPrediction)
        {
            _dataPrediction = dataPrediction;
        }

        /// <summary>Revenue history by month and the forecast for the current month and the next ones.</summary>
        [HttpGet]
        [Route("MonthlyRevenue")]
        public async Task<IActionResult> MonthlyRevenue(int months = 3)
        {
            var response = new Response<SalesForecastDTO>
            {
                status = true,
                data = await _dataPrediction.ForecastMonthlyRevenue(months)
            };
            return Ok(response);
        }
    }
}
