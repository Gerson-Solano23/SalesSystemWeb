using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using System.Net;
namespace SalesSystem.API.Controllers
{
    [Authorize]
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
        [Authorize(Policy = "Admin")]
        [OutputCache(PolicyName = "users")]
        public async Task<IActionResult> List()
        {
            var response = new Response<List<UsuarioDTO>>();
            try
            {
                response.status = true;
                response.data = await _usuarioService.List();

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }


        [HttpGet("getUser")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Get(int id)
        {
            var response = new Response<UsuarioDTO>();
            try
            {
                response.status = true;
                response.data = await _usuarioService.Get(id);

                if (response.data == null)
                {
                    response.status = false;
                    response.message = "User not found";
                    return NotFound(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var response = new Response<SessionDTO>();

            try
            {
                response.status = true;
                response.data = await _usuarioService.ValidateCredentials(login.Email, login.Password);
                if (response.data == null)
                {
                    response.status = false;
                    response.message = "Invalid credentials";
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Create([FromBody] UsuarioDTO entity)
        {
            var response = new Response<UsuarioDTO>();
            try
            {
                response.status = true;
                response.data = await _usuarioService.Create(entity);
                await _outputCacheStore.EvictByTagAsync("users", default);
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
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Update([FromBody] UsuarioDTO entity)
        {
            var response = new Response<bool>();
            try
            {
                response.status = await _usuarioService.Update(entity);
                await _outputCacheStore.EvictByTagAsync("users", default);
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.message = ex.Message;
                return BadRequest(response);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = new Response<bool>();
            try
            {
                response.status = await _usuarioService.Delete(id);
                await _outputCacheStore.EvictByTagAsync("users", default);
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
