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
        [HttpPost("crear")]
        public async Task<IActionResult> CrearSolicitud([FromBody] SolicitudCreateDto dto)
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.CrearSolicitudAsync(dto, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("mis-solicitudes")]
        public async Task<IActionResult> GetMisSolicitudes()
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.GetMisSolicitudesAsync(userId);
            return Ok(result);
        }

        [HttpPost("aceptar-oferta/{ofertaId}")]
        public async Task<IActionResult> AceptarOferta(int ofertaId)
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId)) return Unauthorized();

            var result = await _solicitudService.AceptarOfertaAsync(ofertaId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // --- ASESOR ---
        [HttpGet("disponibles")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> GetDisponibles()
        {
            var result = await _solicitudService.GetSolicitudesDisponiblesAsync();
            return Ok(result);
        }

        [HttpPost("{solicitudId}/ofertar")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> CrearOferta(int solicitudId, [FromBody] OfertaCreateDto dto)
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _solicitudService.CrearOfertaAsync(solicitudId, dto, asesorId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}