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
    public class CategoryController : ControllerBase
    {
        private readonly ICategory _categoryService;

        public CategoryController(ICategory categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var response = new Response<List<CategoryDTO>>
            {
                status = true,
                data = await _categoryService.List()
            };
            return Ok(response);
        }
    }
}
