using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/estudiante")]
    [ApiController]
    // Protegemos todo el controlador. Solo usuarios con estos roles pueden entrar.
    [Authorize(Roles = "Estudiante,Admin")]
    public class EstudianteController : ControllerBase
    {
        private readonly IEstudianteService _estudianteService;

        public EstudianteController(IEstudianteService estudianteService)
        {
            _estudianteService = estudianteService;
        }

        // GET: /api/estudiante/mis-cursos
        [HttpGet("mis-cursos")]
        public async Task<IActionResult> GetMisCursos()
        {
            var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(estudianteId))
            {
                return Unauthorized();
            }

            var misCursos = await _estudianteService.GetMisCursosAsync(estudianteId);
            return Ok(misCursos);
        }
    }
}
