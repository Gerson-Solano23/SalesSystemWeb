using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;

namespace SalesSystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProduct _productService;
        private readonly IOutputCacheStore _outputCacheStore;
        public ProductController(IProduct productService, IOutputCacheStore outputCacheStore)
        {
            _productService = productService;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [Route("List")]
        [OutputCache(PolicyName = "products")]
        // [Authorize(Policy = "Admin_Supervisor")]
        [AllowAnonymous]
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
        [Authorize(Policy = "Admin_Supervisor")]
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
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> Create([FromBody] ProductDTO entity)
        {
            var response = new Response<ProductDTO>();
            try
            {
                response.status = true;
                response.data = await _productService.Create(entity);
                await _outputCacheStore.EvictByTagAsync("products", default);
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
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> Update([FromBody] ProductDTO entity)
        {
            var response = new Response<ProductDTO>();
            try
            {
                response.status = await _productService.Update(entity);
                await _outputCacheStore.EvictByTagAsync("products", default);
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
        [Authorize(Policy = "Admin_Supervisor")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = new Response<bool>();
            try
            {
                response.status = await _productService.Delete(id);
                await _outputCacheStore.EvictByTagAsync("products", default);
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
