using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            var response = new Response<string>();
            try
            {
                if (file == null)
                {
                    response.status = false;
                    response.message = "File not found";
                    return BadRequest(response);
                }

                

                if (await _uploadS3File.UploadFile(file))
                {
                    response.status = true;
                    response.message = "File uploaded successfully";
                    return Ok(response);
                }
                else
                {
                    response.status = false;
                    response.message = "Error uploading file";
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpDelete]
        [Route("DeleteFile")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            var response = new Response<bool>();
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    response.status = false;
                    response.data = false;
                    response.message = "File name not found";
                    return BadRequest(response);
                }

                if (await _uploadS3File.DeleteFile(fileName))
                {
                    response.status = true;
                    response.data = true;
                    response.message = "File deleted successfully";
                    return Ok(response);
                }
                else
                {
                    response.status = false;
                    response.data = false;
                    response.message = "Error deleting file";
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("DownloadFile")]
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var response = new Response<byte[]>();
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    response.status = false;
                    response.message = "File name not found";
                    return BadRequest(response);
                }

                var file = await _uploadS3File.DownloadFile(fileName);
                if (file != null)
                {
                    response.status = true;
                    response.data = file;
                    return Ok(response);
                }
                else
                {
                    response.status = false;
                    response.message = "Error downloading file";
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.Message;
                return BadRequest(response);
            }
        }
    }
}
