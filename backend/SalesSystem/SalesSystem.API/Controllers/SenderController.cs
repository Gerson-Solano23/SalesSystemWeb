using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    /// <summary>
    /// Sends mail from the company account, so it is limited to administrators
    /// and the SMTP credentials are taken from server configuration only.
    /// </summary>
    [Authorize(Policy = "Admin")]
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
        public async Task<IActionResult> SendEmail([FromBody] EmailDTO email)
        {
            var response = new Response<bool>
            {
                status = true,
                data = await _sendEmailService.SendEmail(email)
            };
            return Ok(response);
        }
    }
}
