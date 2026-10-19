using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize(Policy = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly IRol _rolService;

        public RolController(IRol rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var response = new Response<List<RolDTO>>
            {
                status = true,
                data = await _rolService.List()
            };
            return Ok(response);
        }
    }
}
