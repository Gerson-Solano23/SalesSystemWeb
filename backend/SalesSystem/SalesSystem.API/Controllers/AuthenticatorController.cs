using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SalesSystem.API.Utility;
using SalesSystem.BLL.Services.Contract;
using SalesSystem.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SalesSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticatorController : ControllerBase
    {
        private readonly IUsuario _usuarioService;
        private readonly JwtSettings _jwtSettings;

        public AuthenticatorController(IUsuario usuarioService, JwtSettings jwtSettings)
        {
            _usuarioService = usuarioService;
            _jwtSettings = jwtSettings;
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicies.Login)]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var response = new Response<LoginResponseDTO>();
            SessionDTO? session = await _usuarioService.ValidateCredentials(login.Email ?? "", login.Password ?? "");

            if (session == null)
            {
                response.status = false;
                response.message = "Invalid credentials";
                return Unauthorized(response);
            }

            response.status = true;
            response.data = new LoginResponseDTO
            {
                Token = CreateToken(session),
                Session = session
            };
            return Ok(response);
        }

        private string CreateToken(SessionDTO session)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, session.IdUser.ToString()),
                    new Claim(ClaimTypes.Name, session.FullName ?? ""),
                    new Claim(ClaimTypes.Email, session.Email ?? ""),
                    new Claim(ClaimTypes.Role, Roles.ToRoleId(session.RolDescription)),
                }),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                SigningCredentials = new SigningCredentials(_jwtSettings.GetSigningKey(), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}
