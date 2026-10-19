using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize(Policy = "Admin_Supervisor_Employee")]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenu _menuService;

        public MenuController(IMenu menuService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// Menus of the authenticated user. The legacy <c>id</c> query parameter is ignored:
        /// the user always comes from the token, so nobody can read another user's menu.
        /// </summary>
        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var response = new Response<List<MenuDTO>>
            {
                status = true,
                data = await _menuService.List(User.GetUserId())
            };
            return Ok(response);
        }
    }
}
