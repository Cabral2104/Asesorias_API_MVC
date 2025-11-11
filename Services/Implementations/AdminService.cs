using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public AdminService(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<SolicitudAsesorDto>> GetPendingAsesorApplicationsAsync()
        {
            var pendingApplications = await _context.Asesores
                .Where(a => a.EstaAprobado == false && a.IsActive == true)
                .Include(a => a.Usuario)
                .Select(a => new SolicitudAsesorDto // Mapeo completo
                {
                    UsuarioId = a.UsuarioId,
                    UserName = a.Usuario.UserName,
                    Email = a.Usuario.Email,
                    Especialidad = a.Especialidad,
                    Descripcion = a.Descripcion,
                    NivelEstudios = a.NivelEstudios,
                    InstitucionEducativa = a.InstitucionEducativa,
                    CampoEstudio = a.CampoEstudio,
                    AnioGraduacion = a.AnioGraduacion,
                    AniosExperiencia = a.AniosExperiencia,
                    ExperienciaLaboral = a.ExperienciaLaboral,
                    Certificaciones = a.Certificaciones,
                    DocumentoVerificacionUrl = a.DocumentoVerificacionUrl,
                    FechaSolicitud = a.CreatedAt
                })
                .ToListAsync();

            return pendingApplications;
        }

        public async Task<GenericResponseDto> ReviewAsesorApplicationAsync(string userId, bool approve)
        {
            var application = await _context.Asesores.FindAsync(userId);

            if (application == null || application.IsActive == false)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Solicitud no encontrada." };
            }

            if (application.EstaAprobado)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Esta solicitud ya fue aprobada." };
            }

            if (approve)
            {
                application.EstaAprobado = true;
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, "Asesor");
                }
                await _context.SaveChangesAsync();
                return new GenericResponseDto { IsSuccess = true, Message = "Asesor aprobado exitosamente. Rol asignado." };
            }
            else
            {
                _context.Asesores.Remove(application);
                await _context.SaveChangesAsync();
                return new GenericResponseDto { IsSuccess = true, Message = "Solicitud rechazada y archivada." };
            }
        }
    }
}