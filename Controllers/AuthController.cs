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
        // OJO: UserManager<Usuario> ya sabe que Usuario es int gracias al cambio en Program.cs y DbContext
        private readonly UserManager<Usuario> _userManager;

        public AuthController(IAuthService authService, UserManager<Usuario> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistroUsuarioDto registerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.RegisterAsync(registerDto);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _authService.LoginAsync(loginDto);
            if (!result.IsSuccess) return Unauthorized(result);
            return Ok(result);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            // PATRÓN DE PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // Nota: Aquí asumo que tu AuthService.UpdateProfileAsync ahora recibe un int
            // Si no, tendrás que convertir userId.ToString() o actualizar la interfaz IAuthService
            var result = await _authService.UpdateProfileAsync(userId, dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("perfil")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            // PATRÓN DE PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // FindByIdAsync espera string por defecto en Identity, así que convertimos el int a string
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound(new { Message = "Usuario no encontrado." });

            var roles = await _userManager.GetRolesAsync(user);

            var profile = new UserProfileDto
            {
                Id = user.Id, // Devolvemos el ID como string al frontend por compatibilidad JSON
                UserName = user.NombreCompleto ?? user.UserName,
                Email = user.Email,
                Roles = roles.ToList(),
                PhoneNumber = user.PhoneNumber
            };

            return Ok(profile);
        }
    }
}