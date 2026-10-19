using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;

namespace SalesSystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        private readonly IUploadS3File _uploadS3File;

        public UploadFileController(IUploadS3File uploadS3File)
        {
            _uploadS3File = uploadS3File;
        }

        [HttpPost]
        [Route("UploadFile")]
        [Authorize(Policy = "Admin_Supervisor")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile([FromForm] byte[] fileData, string fileName, string fileContentType)
        {
            var response = new Response<string>();
            if (fileData == null || fileData.Length == 0)
            {
                response.status = false;
                response.message = "File not found";
                return BadRequest(response);
            }

            response.status = await _uploadS3File.UploadFile(fileData, fileName, fileContentType);
            response.message = response.status ? "File uploaded successfully" : "Error uploading file";
            return response.status ? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        [Route("DeleteFile")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            var response = new Response<bool>();
            if (string.IsNullOrEmpty(fileName))
            {
                response.status = false;
                response.message = "File name not found";
                return BadRequest(response);
            }

            response.data = response.status = await _uploadS3File.DeleteFile(fileName);
            response.message = response.status ? "File deleted successfully" : "Error deleting file";
            return response.status ? Ok(response) : BadRequest(response);
        }

        [HttpGet]
        [Route("DownloadFile")]
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var response = new Response<byte[]>();
            if (string.IsNullOrEmpty(fileName))
            {
                response.status = false;
                response.message = "File name not found";
                return BadRequest(response);
            }

            response.data = await _uploadS3File.DownloadFile(fileName);
            response.status = response.data != null;
            if (!response.status)
            {
                response.message = "Error downloading file";
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet]
        [Route("getReportsPerMonth")]
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> getReportsPerMonth(string month, string year)
        {
            var response = new Response<List<string>>
            {
                status = true,
                data = await _uploadS3File.getListFilesPerMonth(month, year)
            };
            return Ok(response);
        }
    }
}
