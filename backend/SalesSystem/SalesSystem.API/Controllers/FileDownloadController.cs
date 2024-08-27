using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        private readonly IFileDownload _fileDownloadService;
       

        public FileDownloadController(IFileDownload fileDownloadService)
        {
            _fileDownloadService = fileDownloadService;
            
        }

        [HttpDelete]
        [Route("DeleteFileTask")]
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> DeleteFile(int idUser)
        {
            var response = new Response<bool>();

            try
            {
                response.status = true;
                response.data =  _fileDownloadService.DeleteFile(idUser);

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("DownloadExcel")]
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> DownloadExcel(int idUser)
        {
            var response = new Response<byte[]>();
            

            try
            {
                response.status = true;
                response.data = await _fileDownloadService.getFileExcel(idUser);

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
