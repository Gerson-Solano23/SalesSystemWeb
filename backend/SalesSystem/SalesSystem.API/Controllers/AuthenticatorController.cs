using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SalesSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticatorController : ControllerBase
    {
        private static readonly string? key = Environment.GetEnvironmentVariable("CERTIFICATE_KEY");
        private readonly IUsuario _usuarioService;
        public AuthenticatorController(IUsuario Iuser)
        {
            this._usuarioService = Iuser;
        }

        [HttpGet]
        [Route("getToken")]
        public async Task<IActionResult> get(string email, string password)
        {
            var response = new Response<string>();
            try
            {
                SessionDTO session = new SessionDTO();
                session = await _usuarioService.ValidateCredentials(email, password);
                if (session != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var byteKey = Encoding.UTF8.GetBytes(key);
                    var tekenDesc = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, session.FullName),
                            new Claim(ClaimTypes.Role, await getIdRolNumber(session.RolDescription)),
                        }),
                        Expires = DateTime.UtcNow.AddHours(10.5),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tekenDesc);
                    response.status = true;
                    response.data = tokenHandler.WriteToken(token);
                    if (response.data == null)
                    {
                        response.status = false;
                        response.message = "Invalid credentials";
                    }
                    return Ok(response);

                }
                else
                {
                    return Unauthorized("Acceso no autorizado: El usuario no se encuentra registrado");
                }
            }
            catch (Exception ex)
            {

                response.status = false;
                response.message = ex.Message;
                return BadRequest(response);
            }

        }

        private async Task<string> getIdRolNumber(string rolDescription)
        {
            string idRol = "";
            try
            {
                if (rolDescription == "Admin")
                {
                    idRol = "1";
                }
                else if (rolDescription == "Employee")
                {
                    idRol = "2";
                }
                else if (rolDescription == "Supervisor")
                {
                    idRol = "3"; 
                }
            }
            catch (Exception)
            {

                throw;
            }
            return idRol;
        }

    }
}
