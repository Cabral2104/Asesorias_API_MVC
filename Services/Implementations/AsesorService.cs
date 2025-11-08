using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class AsesorService : IAsesorService
    {
        private readonly ApplicationDbContext _context;

        public AsesorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GenericResponseDto> ApplyToBeAsesorAsync(AsesorApplyDto dto, string userId)
        {
            var existingApplication = await _context.Asesores.FindAsync(userId);
            if (existingApplication != null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Ya tienes una solicitud pendiente o aprobada." };
            }

            // --- ¡MAPEO ACTUALIZADO! ---
            var newAsesorApplication = new Asesor
            {
                UsuarioId = userId,
                Especialidad = dto.Especialidad,
                Descripcion = dto.Descripcion,

                // Nuevos campos académicos
                NivelEstudios = dto.NivelEstudios,
                InstitucionEducativa = dto.InstitucionEducativa,
                CampoEstudio = dto.CampoEstudio,
                AnioGraduacion = dto.AnioGraduacion,

                // Nuevos campos de experiencia
                AniosExperiencia = dto.AniosExperiencia,
                ExperienciaLaboral = dto.ExperienciaLaboral,
                Certificaciones = dto.Certificaciones,

                // La URL (el parche temporal)
                DocumentoVerificacionUrl = dto.DocumentoVerificacionUrl,
                EstaAprobado = false

                // IsActive, CreatedAt, ModifiedAt se llenan automáticamente
            };

            await _context.Asesores.AddAsync(newAsesorApplication);
            await _context.SaveChangesAsync();

            return new GenericResponseDto
            {
                IsSuccess = true,
                Message = "¡Solicitud enviada! Un administrador la revisará pronto."
            };
        }
    }
}
