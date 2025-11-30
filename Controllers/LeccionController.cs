using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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

        [HttpGet("curso/{cursoId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLecciones(int cursoId)
        {
            var lecciones = await _leccionService.GetLeccionesByCursoIdAsync(cursoId);
            return Ok(lecciones);
        }

        [HttpPost("curso/{cursoId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> AddLeccion(int cursoId, [FromBody] LeccionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _leccionService.AddLeccionToCursoAsync(cursoId, dto, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("{leccionId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> UpdateLeccion(int leccionId, [FromBody] LeccionCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _leccionService.UpdateLeccionAsync(leccionId, dto, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{leccionId}")]
        [Authorize(Roles = "Asesor")]
        public async Task<IActionResult> DeleteLeccion(int leccionId)
        {
            // PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var result = await _leccionService.DeleteLeccionAsync(leccionId, asesorId);

            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }
    }
}