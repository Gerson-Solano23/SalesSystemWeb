using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using System.Security.Claims;

namespace SalesSystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
       private readonly IMenu _menuService;
        private readonly IUsuario _usuarioService;

        public MenuController(IMenu menuService, IUsuario usuarioService)
        {
            _menuService = menuService;
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Route("List")]
        [Authorize(Policy = "Admin_Supervisor_Employee")]
        public async Task<IActionResult> List(int id)
        {
            var roles = User.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value);
            var user = _usuarioService.Get(id).Result;
            if (user.IdRol.ToString() != roles.First())
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }
            var response = new Response<List<MenuDTO>>();
            try
            {
                response.status = true;
                response.data = await _menuService.List(id);

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
