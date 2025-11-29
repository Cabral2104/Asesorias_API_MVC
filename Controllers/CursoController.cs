using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/curso")]
    [ApiController]
    public class CursoController : ControllerBase
    {
        private readonly ICursoService _cursoService;
        private readonly IEstudianteService _estudianteService;
        private readonly ICalificacionService _calificacionService;

        public CursoController(ICursoService cursoService, IEstudianteService estudianteService, ICalificacionService calificacionService)
        {
            _cursoService = cursoService;
            _estudianteService = estudianteService;
            _calificacionService = calificacionService;
        }

        [HttpGet("publicos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCursosPublicos()
        {
            var cursos = await _cursoService.GetCursosPublicosAsync();
            return Ok(cursos);
        }

        [HttpPost("crear")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> CreateCurso([FromBody] CursoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _cursoService.CreateCursoAsync(dto, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("publish/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> PublishCurso(int cursoId)
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _cursoService.PublishCursoAsync(cursoId, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("mis-cursos")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> GetMyCursos()
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var cursos = await _cursoService.GetMyCursosAsync(asesorId);
            return Ok(cursos);
        }

        [HttpPut("actualizar/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> UpdateCurso(int cursoId, [FromBody] CursoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _cursoService.UpdateCursoAsync(cursoId, dto, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("eliminar/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> DeleteCurso(int cursoId)
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _cursoService.DeleteCursoAsync(cursoId, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{cursoId}/inscribirme")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> InscribirseACurso(int cursoId)
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId)) return Unauthorized();

            var result = await _estudianteService.InscribirseACursoAsync(cursoId, estudianteId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("{cursoId}/calificar")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> CalificarCurso(int cursoId, [FromBody] CalificacionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId)) return Unauthorized();

            var result = await _calificacionService.AddCalificacionAsync(cursoId, estudianteId, dto);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}