using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Implementations;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/curso")] // Ruta base: /api/curso
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
            _estudianteService = estudianteService;
            _calificacionService = calificacionService;
        }

        // --- Endpoint 1: Ver Catálogo (PÚBLICO) ---
        // GET: /api/curso/publicos
        [HttpGet("publicos")]
        [AllowAnonymous] // ¡Todos pueden ver esto, no se necesita token!
        public async Task<IActionResult> GetCursosPublicos()
        {
            var cursos = await _cursoService.GetCursosPublicosAsync();
            return Ok(cursos);
        }

        // --- Endpoint 2: Crear Curso (SOLO ASESORES) ---
        // POST: /api/curso/crear
        [HttpPost("crear")]
        [Authorize(Roles = "Asesor")] // ¡Solo Asesores Aprobados!
        public async Task<IActionResult> CreateCurso([FromBody] CursoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtenemos el ID del Asesor desde el token
            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(asesorId))
            {
                return Unauthorized();
            }

            var result = await _cursoService.CreateCursoAsync(dto, asesorId);

            if (!result.IsSuccess)
            {
                return BadRequest(result); // Ej: "No eres un asesor aprobado"
            }

            return Ok(result);
        }

        // --- Endpoint 3: Publicar Curso (SOLO ASESORES) ---
        // POST: /api/curso/publish/{cursoId}
        [HttpPost("publish/{cursoId}")]
        [Authorize(Roles = "Asesor")] // ¡Solo Asesores Aprobados!
        public async Task<IActionResult> PublishCurso(int cursoId)
        {
            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(asesorId))
            {
                return Unauthorized();
            }

            var result = await _cursoService.PublishCursoAsync(cursoId, asesorId);

            if (!result.IsSuccess)
            {
                // Ej: "No eres el dueño de este curso"
                return BadRequest(result);
            }

            return Ok(result);
        }

        // --- Endpoint 4: Ver mis Cursos (SOLO ASESORES) ---
        // GET: /api/curso/mis-cursos
        [HttpGet("mis-cursos")]
        [Authorize(Roles = "Asesor")] // ¡Solo Asesores Aprobados!
        public async Task<IActionResult> GetMyCursos()
        {
            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(asesorId))
            {
                return Unauthorized();
            }

            var cursos = await _cursoService.GetMyCursosAsync(asesorId);
            return Ok(cursos);
        }

        // --- Endpoint 5: Actualizar mi Curso (SOLO ASESORES) ---
        // PUT: /api/curso/actualizar/{cursoId}
        [HttpPut("actualizar/{cursoId}")]
        [Authorize(Roles = "Asesor")] // ¡Solo Asesores Aprobados!
        public async Task<IActionResult> UpdateCurso(int cursoId, [FromBody] CursoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(asesorId))
            {
                return Unauthorized();
            }

            var result = await _cursoService.UpdateCursoAsync(cursoId, dto, asesorId);

            if (!result.IsSuccess)
            {
                // Ej: "No eres el dueño de este curso"
                return BadRequest(result);
            }

            return Ok(result);
        }

        // --- Endpoint 6: Eliminar mi Curso (SOLO ASESORES) ---
        // DELETE: /api/curso/eliminar/{cursoId}
        [HttpDelete("eliminar/{cursoId}")]
        [Authorize(Roles = "Asesor")] // ¡Solo Asesores Aprobados!
        public async Task<IActionResult> DeleteCurso(int cursoId)
        {
            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(asesorId))
            {
                return Unauthorized();
            }

            var result = await _cursoService.DeleteCursoAsync(cursoId, asesorId);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        // Endpoint 7: Inscribirse a un Curso (SOLO ESTUDIANTES)
        // POST: /api/curso/{cursoId}/inscribirme
        [HttpPost("{cursoId}/inscribirme")]
        [Authorize(Roles = "Estudiante")] // ¡Solo Estudiantes!
        public async Task<IActionResult> InscribirseACurso(int cursoId)
        {
            var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(estudianteId))
            {
                return Unauthorized();
            }

            var result = await _estudianteService.InscribirseACursoAsync(cursoId, estudianteId);

            if (!result.IsSuccess)
            {
                return BadRequest(result); // Ej: "Ya estás inscrito"
            }

            return Ok(result);
        }

        // Endpoint 8: Calificar un Curso (SOLO ESTUDIANTES)
        // POST: /api/curso/{cursoId}/calificar
        [HttpPost("{cursoId}/calificar")]
        [Authorize(Roles = "Estudiante")] // ¡Solo Estudiantes!
        public async Task<IActionResult> CalificarCurso(int cursoId, [FromBody] CalificacionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var estudianteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(estudianteId))
            {
                return Unauthorized();
            }

            var result = await _calificacionService.AddCalificacionAsync(cursoId, estudianteId, dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result); // Ej: "Ya calificaste este curso"
            }

            return Ok(result);
        }
    }
}
