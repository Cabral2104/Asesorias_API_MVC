using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using System.Text.Json;
using System.IO;

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
        [DisableRequestSizeLimit] // Desactivar límite de tamaño para este endpoint
        public async Task<IActionResult> ApplyToBeAsesor()
        {
            // 1. LECTURA MANUAL DEL CUERPO (Evita el crash del ModelBinder)
            string requestBody = "";
            try
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error leyendo datos: {ex.Message}");
            }

            // 2. DESERIALIZACIÓN MANUAL
            AsesorApplyDto dto;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                dto = JsonSerializer.Deserialize<AsesorApplyDto>(requestBody, options);
            }
            catch (Exception)
            {
                return BadRequest("El formato de los datos enviados no es válido.");
            }

            // 3. VALIDACIONES DE NEGOCIO
            if (dto == null) return BadRequest("No se recibieron datos.");

            if (string.IsNullOrEmpty(dto.ArchivoBase64))
            {
                return BadRequest(new { IsSuccess = false, Message = "Debes adjuntar tu CV." });
            }

            // 4. OBTENER USUARIO
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 5. LLAMAR AL SERVICIO
            try
            {
                var result = await _asesorService.ApplyToBeAsesorAsync(dto, userId);

                if (!result.IsSuccess) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error interno: {ex.Message}");
                return StatusCode(500, new { IsSuccess = false, Message = "Error interno del servidor." });
            }
        }
    }
}
