using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgresoController : ControllerBase
    {
        private readonly IProgresoService _progresoService;

        public ProgresoController(IProgresoService progresoService)
        {
            _progresoService = progresoService;
        }

        [HttpPost("marcar/{leccionId}")]
        public async Task<IActionResult> MarcarLeccion(int leccionId, [FromBody] bool completada)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId)) return Unauthorized();

            var result = await _progresoService.MarcarLeccionAsync(estudianteId, leccionId, completada);
            return Ok(result);
        }

        [HttpGet("curso/{cursoId}")]
        public async Task<IActionResult> GetProgresoCurso(int cursoId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId)) return Unauthorized();

            var progreso = await _progresoService.ObtenerProgresoCursoAsync(estudianteId, cursoId);
            return Ok(progreso);
        }
    }
}