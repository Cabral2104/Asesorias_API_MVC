using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Asesorias_API_MVC.Models;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<Usuario> _userManager;

        // Inyectamos el servicio
        public AuthController(IAuthService authService, UserManager<Usuario> userManager)
        {
            _authService = authService;
            _userManager = userManager;
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

        [HttpGet("perfil")]
        [Authorize] // <-- ¡LA CERRADURA! Solo usuarios autenticados pueden entrar.
        public async Task<IActionResult> GetMyProfile()
        {
            // Gracias a [Authorize], podemos estar seguros de que "User" existe.
            // Leemos el ID del token (que es más seguro que pasarlo por URL).
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "Usuario no encontrado." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Devolvemos el perfil usando nuestro DTO
            var profile = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Roles = roles.ToList()
            };

            return Ok(profile);
        }
    }
}

