using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SaleController : ControllerBase
    {
        private readonly ISale _saleService;
        private readonly IFileDownload _fileDownload;

        public SaleController(ISale saleService, IFileDownload fileDownload)
        {
            _saleService = saleService;
            _fileDownload = fileDownload;
        }

        [HttpGet]
        [Route("History")]
        [Authorize(Policy = "Admin_Supervisor_Employee")]
        [EnableRateLimiting(RateLimitPolicies.Fixed)]
        public async Task<IActionResult> History(string searchBy, string? saleNumber, string? Startdate, string? endDate)
        {
            var response = new Response<List<SaleDTO>>
            {
                status = true,
                data = await _saleService.History(searchBy, saleNumber ?? "", Startdate ?? "", endDate ?? "")
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("Report")]
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> Report(string? Startdate, string? endDate)
        {
            var response = new Response<List<ReportDTO>>
            {
                status = true,
                data = await _saleService.Report(Startdate ?? "", endDate ?? "")
            };
            _ = Task.Run(() => _fileDownload.runTaskFileExcel(User.GetUserId()));
            return Ok(response);
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Policy = "Admin_Supervisor_Employee")]
        public async Task<IActionResult> Create([FromBody] SaleDTO entity)
        {
            var response = new Response<SaleDTO>
            {
                status = true,
                data = await _saleService.Create(entity)
            };
            return Ok(response);
        }
    }
}
