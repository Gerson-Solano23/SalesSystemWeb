using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using SalesSystem.Utility;

namespace SalesSystem.API.Controllers
{
    [Authorize(Policy = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _usuarioService;
        private readonly IOutputCacheStore _outputCacheStore;

        public UsuarioController(IUsuario usuarioService, IOutputCacheStore outputCacheStore)
        {
            _outputCacheStore = outputCacheStore;
            _usuarioService = usuarioService;
        }

        [HttpGet("List")]
        [OutputCache(PolicyName = "users")]
        public async Task<IActionResult> List()
        {
            var response = new Response<List<UsuarioDTO>>
            {
                status = true,
                data = await _usuarioService.List()
            };
            return Ok(response);
        }

        [HttpGet("getUser")]
        public async Task<IActionResult> Get(int id)
        {
            var response = new Response<UsuarioDTO>
            {
                status = true,
                data = await _usuarioService.Get(id)
            };
            return Ok(response);
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create([FromBody] UsuarioDTO entity)
        {
            var response = new Response<UsuarioDTO>
            {
                status = true,
                data = await _usuarioService.Create(entity)
            };
            await _outputCacheStore.EvictByTagAsync("users", default);
            return Ok(response);
        }

        [HttpPut]
        [Route("Update")]
        public async Task<IActionResult> Update([FromBody] UsuarioDTO entity)
        {
            var response = new Response<bool>
            {
                status = await _usuarioService.Update(entity)
            };
            await _outputCacheStore.EvictByTagAsync("users", default);
            return Ok(response);
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == User.GetUserId())
            {
                throw new BusinessException("You cannot delete your own user");
            }

            var response = new Response<bool>
            {
                status = await _usuarioService.Delete(id)
            };
            await _outputCacheStore.EvictByTagAsync("users", default);
            return Ok(response);
        }
    }
}
