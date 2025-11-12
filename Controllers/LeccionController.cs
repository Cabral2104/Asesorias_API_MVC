using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/leccion")]
    [ApiController]
    public class LeccionController : ControllerBase
    {
        private readonly ILeccionService _leccionService;

        public LeccionController(ILeccionService leccionService)
        {
            _leccionService = leccionService;
        }

        // --- Endpoint 1: Ver Lecciones de un Curso (PÚBLICO) ---
        // GET: /api/leccion/curso/{cursoId}
        [HttpGet("curso/{cursoId}")]
        [AllowAnonymous] // Todos pueden ver las lecciones de un curso público
        public async Task<IActionResult> GetLecciones(int cursoId)
        {
            var lecciones = await _leccionService.GetLeccionesByCursoIdAsync(cursoId);
            return Ok(lecciones);
        }

        // --- Endpoint 2: Agregar Lección a un Curso (SOLO ASESOR) ---
        // POST: /api/leccion/curso/{cursoId}
        [HttpPost("curso/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> AddLeccion(int cursoId, [FromBody] LeccionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _leccionService.AddLeccionToCursoAsync(cursoId, dto, asesorId);

            if (!result.IsSuccess)
            {
                return BadRequest(result); // Ej: "No eres el dueño de este curso"
            }
            return Ok(result);
        }

        // --- Endpoint 3: Actualizar Lección (SOLO ASESOR) ---
        // PUT: /api/leccion/{leccionId}
        [HttpPut("{leccionId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> UpdateLeccion(int leccionId, [FromBody] LeccionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _leccionService.UpdateLeccionAsync(leccionId, dto, asesorId);

            if (!result.IsSuccess)
            {
                return BadRequest(result); // Ej: "No eres el dueño de esta lección"
            }
            return Ok(result);
        }

        // --- Endpoint 4: Eliminar Lección (SOLO ASESOR) ---
        // DELETE: /api/leccion/{leccionId}
        [HttpDelete("{leccionId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> DeleteLeccion(int leccionId)
        {
            var asesorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _leccionService.DeleteLeccionAsync(leccionId, asesorId);

            if (!result.IsSuccess)
            {
                return BadRequest(result); // Ej: "No eres el dueño de esta lección"
            }
            return Ok(result);
        }
    }
}
