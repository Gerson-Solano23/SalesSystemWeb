using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using System.Net;
namespace SalesSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuario _usuarioService;

        public UsuarioController(IUsuario usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet("List")]
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
        public async Task<IActionResult> Create(UsuarioDTO entity)
        {
            var response = new Response<UsuarioDTO>();
            try
            {
                response.status = true;
                response.data = await _usuarioService.Create(entity);

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
        public async Task<IActionResult> Update(UsuarioDTO entity)
        {
            var response = new Response<bool>();
            try
            {
                response.status = await _usuarioService.Update(entity);

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
        public async Task<IActionResult> Delete(int id)
        {
            var response = new Response<bool>();
            try
            {
                response.status = await _usuarioService.Delete(id);

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
