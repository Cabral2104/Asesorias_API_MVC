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

        [HttpPost("apply")]
        [Consumes("multipart/form-data")] // IMPORTANTE: Aceptamos archivos
        public async Task<IActionResult> ApplyToBeAsesor([FromForm] AsesorApplyDto dto)
        {
            // Validación manual del archivo
            if (dto.DocumentoVerificacion == null || dto.DocumentoVerificacion.Length == 0)
            {
                return BadRequest(new { Message = "Debes subir tu CV en formato PDF o DOCX." });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _asesorService.ApplyToBeAsesorAsync(dto, userId);

            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
    }
}
