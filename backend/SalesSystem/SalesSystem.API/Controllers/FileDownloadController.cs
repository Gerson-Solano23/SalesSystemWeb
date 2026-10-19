using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;

namespace SalesSystem.API.Controllers
{
    [Authorize(Policy = "Admin_Supervisor")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        private readonly IFileDownload _fileDownloadService;
        private readonly ISale _saleService;

        public FileDownloadController(IFileDownload fileDownloadService, ISale saleService)
        {
            _fileDownloadService = fileDownloadService;
            _saleService = saleService;
        }

        /// <summary>
        /// Builds the Excel report for the date range in this same request.
        /// Nothing is cached on the server, so users never get each other's reports.
        /// </summary>
        [HttpGet]
        [Route("DownloadExcel")]
        public async Task<IActionResult> DownloadExcel(string Startdate, string endDate)
        {
            var report = await _saleService.Report(Startdate, endDate);
            var response = new Response<byte[]>
            {
                status = true,
                data = await _fileDownloadService.GetExcelFile(report)
            };
            return Ok(response);
        }
    }
}
