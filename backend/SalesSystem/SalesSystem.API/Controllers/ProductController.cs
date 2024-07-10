using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProduct _productService;

        public ProductController(IProduct productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var response = new Response<List<ProductDTO>>();
            try
            {
                response.status = true;
                response.data = await _productService.List();

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("Get")]
        public async Task<IActionResult> Get(int id)
        {
            var response = new Response<ProductDTO>();
            try
            {
                response.status = true;
                response.data = await _productService.Get(id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(ProductDTO entity)
        {
            var response = new Response<ProductDTO>();
            try
            {
                response.status = true;
                response.data = await _productService.Create(entity);

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpPut]
        [Route("Update")]
        public async Task<IActionResult> Update(ProductDTO entity)
        {
            var response = new Response<ProductDTO>();
            try
            {
                response.status = await _productService.Update(entity);
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = new Response<bool>();
            try
            {
                response.status = await _productService.Delete(id);
                return Ok(response);
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
