using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/solicitud")]
    [ApiController]
    [Authorize]
    public class SolicitudController : ControllerBase
    {
        private readonly ISolicitudService _solicitudService;

        public SolicitudController(ISolicitudService solicitudService)
        {
            _solicitudService = solicitudService;
        }

        // --- ESTUDIANTE ---

        [HttpPost("custom/crear")] // <--- IMPORTANTE: custom/crear
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> CrearSolicitudCustom([FromBody] SolicitudCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.CrearSolicitudAsync(dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("custom/mis-solicitudes")] // <--- IMPORTANTE: custom/mis-solicitudes
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> GetMisSolicitudesCustom()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.GetMisSolicitudesAsync(userId);
            return Ok(result);
        }

        [HttpPost("custom/aceptar-oferta/{ofertaId}")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> AceptarOfertaCustom(int ofertaId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.AceptarOfertaAsync(ofertaId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // --- ASESOR ---
        [HttpGet("custom/mercado")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> GetMercadoSolicitudes([FromQuery] string? materia)
        {
            var result = await _solicitudService.GetSolicitudesDisponiblesAsync(materia);
            return Ok(result);
        }

        [HttpPost("custom/{solicitudId}/ofertar")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> CrearOfertaCustom(int solicitudId, [FromBody] OfertaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _solicitudService.CrearOfertaAsync(solicitudId, dto, asesorId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("custom/actualizar/{id}")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> UpdateSolicitud(int id, [FromBody] SolicitudUpdateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.UpdateSolicitudAsync(id, dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("custom/eliminar/{id}")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> DeleteSolicitud(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.DeleteSolicitudAsync(id, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("custom/finalizar/{id}")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> FinalizarSolicitud(int id, [FromBody] FinalizarSolicitudDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.FinalizarSolicitudAsync(id, dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}