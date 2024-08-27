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
    public class SaleController : ControllerBase
    {
        private readonly ISale _saleService;
        private readonly IFileDownload _fileDownload;
        //public List<ReportDTO> ReportTEMP = new List<ReportDTO>();
        public SaleController(ISale saleService, IFileDownload fileDownload)
        {
            _saleService = saleService;
            _fileDownload = fileDownload;
        }

        [HttpGet]
        [Route("History")]
        [Authorize(Policy = "Admin_Supervisor_Employee")]
        public async Task<IActionResult> History(string searchBy, string? saleNumber, string? Startdate, string? endDate)
        {
            var response = new Response<List<SaleDTO>>();
            saleNumber = saleNumber is null ? "" : saleNumber;
            Startdate = Startdate is null ? "" : Startdate;
            endDate = endDate is null ? "" : endDate;
            try
            {
                response.status = true;
                response.data = await _saleService.History(searchBy, saleNumber, Startdate, endDate);

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }
        [HttpGet]
        [Route("Report")]
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> Report(string? Startdate, string? endDate, int idUser)
        {
            var response = new Response<List<ReportDTO>>();

            endDate = endDate is null ? "" : endDate;
            try
            {
                response.status = true;
                response.data = await _saleService.Report(Startdate, endDate);
                Task.Run(() => _fileDownload.runTaskFileExcel(idUser));
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }
        [HttpPost]
        [Route("Create")]
        [Authorize(Policy = "Admin_Supervisor_Employee")]
        public async Task<IActionResult> Create([FromBody] SaleDTO entity)
        {
            var response = new Response<SaleDTO>();

            try
            {
                response.status = true;
                response.data = await _saleService.Create(entity);

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
