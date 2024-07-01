using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolController : ControllerBase
    {
        private readonly IRol _rolRepository;

        public RolController(IRol rolRepository)
        {
            _rolRepository = rolRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var response = new Response<List<RolDTO>>();
            try
            {
                response.status = true;
                response.data = await _rolRepository.List();

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
