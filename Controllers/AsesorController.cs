using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AsesorController : ControllerBase
    {
        private readonly IAsesorService _asesorService;

        public AsesorController(IAsesorService asesorService)
        {
            _asesorService = asesorService;
        }

        // Endpoint para solicitar ser Asesor
        // POST: /api/Asesor/apply
        [HttpPost("apply")]
        // --- CAMBIO CLAVE ---
        // Volvemos a [FromBody] y aceptamos JSON.
        // Quitamos [Consumes], [DisableRequestSizeLimit], etc.
        public async Task<IActionResult> ApplyToBeAsesor([FromBody] AsesorApplyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // --- CAMBIO CLAVE ---
            // Ya no pasamos el archivo, solo el DTO
            var result = await _asesorService.ApplyToBeAsesorAsync(dto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
