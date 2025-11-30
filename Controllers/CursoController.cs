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

        public CursoController(
            ICursoService cursoService,
            IEstudianteService estudianteService,
            ICalificacionService calificacionService)
        {
            _cursoService = cursoService;
            _estudianteService = estudianteService;
            _calificacionService = calificacionService;
        }

        // ============================================================
        //                        PÚBLICO
        // ============================================================

        // GET: /api/curso/publicos
        [HttpGet("publicos")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCursosPublicos()
        {
            var cursos = await _cursoService.GetCursosPublicosAsync();
            return Ok(cursos);
        }

        // ============================================================
        //                  GESTIÓN DEL ASESOR
        // ============================================================

        // POST: /api/curso/crear
        [HttpPost("crear")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> CreateCurso([FromBody] CursoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId))
                return Unauthorized();

            var result = await _cursoService.CreateCursoAsync(dto, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // POST: /api/curso/publish/{cursoId}
        [HttpPost("publish/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> PublishCurso(int cursoId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId))
                return Unauthorized();

            var result = await _cursoService.PublishCursoAsync(cursoId, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // GET: /api/curso/mis-cursos
        [HttpGet("mis-cursos")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> GetMyCursos()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId))
                return Unauthorized();

            var cursos = await _cursoService.GetMyCursosAsync(asesorId);
            return Ok(cursos);
        }

        // GET: /api/curso/detalle/{id}  (Para edición)
        [HttpGet("detalle/{id}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> GetCursoDetail(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId))
                return Unauthorized();

            var curso = await _cursoService.GetCursoByIdForAsesorAsync(id, asesorId);

            if (curso == null) return NotFound(new { Message = "Curso no encontrado o no te pertenece." });

            return Ok(curso);
        }

        // PUT: /api/curso/actualizar/{cursoId}
        [HttpPut("actualizar/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> UpdateCurso(int cursoId, [FromBody] CursoCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId))
                return Unauthorized();

            var result = await _cursoService.UpdateCursoAsync(cursoId, dto, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // DELETE: /api/curso/eliminar/{cursoId}
        [HttpDelete("eliminar/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> DeleteCurso(int cursoId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId))
                return Unauthorized();

            var result = await _cursoService.DeleteCursoAsync(cursoId, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // GET: /api/curso/{cursoId}/calificaciones (Ver reseñas de mi curso)
        [HttpGet("{cursoId}/calificaciones")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> GetCalificacionesCurso(int cursoId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId))
                return Unauthorized();

            var calificaciones = await _calificacionService.GetCalificacionesCursoAsync(cursoId, asesorId);
            return Ok(calificaciones);
        }

        // ============================================================
        //                  ACCIONES DEL ESTUDIANTE
        // ============================================================

        // POST: /api/curso/inscribirse (Con datos de pago seguros)
        [HttpPost("inscribirse")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> InscribirseACurso([FromBody] InscripcionPagoDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId))
                return Unauthorized();

            var result = await _estudianteService.InscribirseACursoAsync(dto, estudianteId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // POST: /api/curso/{cursoId}/calificar
        [HttpPost("{cursoId}/calificar")]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> CalificarCurso(int cursoId, [FromBody] CalificacionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int estudianteId))
                return Unauthorized();

            var result = await _calificacionService.AddCalificacionAsync(cursoId, estudianteId, dto);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}