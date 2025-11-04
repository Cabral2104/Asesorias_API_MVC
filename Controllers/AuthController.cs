using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Inyectamos el servicio (el "Chef")
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Endpoint para Registrar
        // POST: /api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroUsuarioDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Devuelve error si el DTO no es válido
            }

            // El controlador solo le pasa el trabajo al servicio
            var result = await _authService.RegisterAsync(registerDto);

            // Y devuelve la respuesta del servicio
            if (!result.IsSuccess)
            {
                return BadRequest(result); // Si falló (ej: email existe)
            }

            return Ok(result); // Si fue exitoso
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
                return Unauthorized(result); // Usamos 401 Unauthorized para login fallido
            }

            return Ok(result); // 200 OK con el token
        }
    }
}
