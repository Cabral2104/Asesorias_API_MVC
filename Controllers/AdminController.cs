using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asesorias_API_MVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // ¡Protegido!
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("pending-applications")]
        public async Task<IActionResult> GetPendingApplications()
        {
            var applications = await _adminService.GetPendingAsesorApplicationsAsync();
            return Ok(applications);
        }

        [HttpPost("approve/{userId}")]
        public async Task<IActionResult> ApproveApplication(string userId)
        {
            var result = await _adminService.ReviewAsesorApplicationAsync(userId, true);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("reject/{userId}")]
        public async Task<IActionResult> RejectApplication(string userId)
        {
            var result = await _adminService.ReviewAsesorApplicationAsync(userId, false);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        // --- ENDPOINTS DEL DASHBOARD ---
        // GET: /api/Admin/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        // GET: /api/Admin/dashboard-asesores
        [HttpGet("dashboard-asesores")]
        public async Task<IActionResult> GetAsesorDashboard()
        {
            var stats = await _adminService.GetAsesorDashboardAsync();
            return Ok(stats);
        }

        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart()
        {
            var data = await _adminService.GetMonthlyRevenueAsync();
            return Ok(data);
        }

        [HttpGet("asesor-detail/{asesorId}")]
        public async Task<IActionResult> GetAsesorDetail(string asesorId)
        {
            var data = await _adminService.GetAsesorDetailsAsync(asesorId);
            if (data == null) return NotFound();
            return Ok(data);
        }
    }
}
