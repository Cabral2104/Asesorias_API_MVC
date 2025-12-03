using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/estudiante")]
    [ApiController]
    [Authorize(Roles = "Estudiante,Admin")]
    public class EstudianteController : ControllerBase
    {
        private readonly IEstudianteService _estudianteService;

        public EstudianteController(IEstudianteService estudianteService)
        {
            _estudianteService = estudianteService;
        }

        [HttpGet("mis-cursos")]
        public async Task<IActionResult> GetMisCursos()
        {
            // PATRÓN DE PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId))
            {
                return Unauthorized();
            }

            var misCursos = await _estudianteService.GetMisCursosAsync(estudianteId);
            return Ok(misCursos);
        }

        [HttpGet("pagos")]
        public async Task<IActionResult> GetHistorialPagos()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId)) return Unauthorized();

            var pagos = await _estudianteService.GetHistorialPagosAsync(userId);
            return Ok(pagos);
        }
    }
}