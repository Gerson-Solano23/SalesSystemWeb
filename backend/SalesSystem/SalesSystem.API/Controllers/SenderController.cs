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
    public class SenderController : ControllerBase
    {
        private readonly ISendEmail _sendEmailService;
        public SenderController(ISendEmail sendEmailService)
        {
            _sendEmailService = sendEmailService;
        }

        
        [HttpPost]
        [Route("SendEmail")]
        [Authorize(Policy = "Admin_Supervisor_Employee")]
        public async Task<IActionResult> SendEmail([FromBody] EmailDTO email)
        {
            var response = new Response<bool>();
            try
            {
                response.status = true;
                response.data = await _sendEmailService.SendEmail(email);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }
    }
}
