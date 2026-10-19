using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;

namespace SalesSystem.API.Controllers
{
    /// <summary>
    /// Excel report generated for the authenticated user. The legacy <c>idUser</c> query
    /// parameter is ignored: the user always comes from the token.
    /// </summary>
    [Authorize(Policy = "Admin_Supervisor")]
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
        public IActionResult DeleteFile()
        {
            var response = new Response<bool>
            {
                status = true,
                data = _fileDownloadService.DeleteFile(User.GetUserId())
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("DownloadExcel")]
        public async Task<IActionResult> DownloadExcel()
        {
            var response = new Response<byte[]>
            {
                status = true,
                data = await _fileDownloadService.getFileExcel(User.GetUserId())
            };
            return Ok(response);
        }
    }
}
