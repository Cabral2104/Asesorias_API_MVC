using Microsoft.AspNetCore.Mvc;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AsesorController : ControllerBase
    {
        private readonly IAsesorService _asesorService;

        public AsesorController(IAsesorService asesorService)
        {
            _asesorService = asesorService;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyToBeAsesor([FromBody] AsesorApplyDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // PATRÓN DE PARSING INT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            try
            {
                var result = await _asesorService.ApplyToBeAsesorAsync(dto, userId);

                if (!result.IsSuccess) return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = "Error interno: " + ex.Message });
            }
        }

        [HttpGet("chart-data")]
        public async Task<IActionResult> GetChartData()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            try
            {
                var data = await _asesorService.GetActivityChartAsync(asesorId);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error calculando estadísticas: " + ex.Message });
            }
        }

        [HttpGet("mis-trabajos")]
        public async Task<IActionResult> GetMyJobs()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int asesorId)) return Unauthorized();

            var jobs = await _asesorService.GetMyJobsAsync(asesorId);
            return Ok(jobs);
        }

        [HttpGet("public/{id}")]
        [AllowAnonymous] // ¡Importante! Cualquiera puede verlo
        public async Task<IActionResult> GetPublicProfile(int id)
        {
            var profile = await _asesorService.GetPublicProfileAsync(id);
            if (profile == null) return NotFound();
            return Ok(profile);
        }
    }
}