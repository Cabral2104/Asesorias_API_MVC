using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore; // Necesario para IgnoreQueryFilters

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
            // 1. BUSCAR SI YA EXISTE (INCLUYENDO LOS BORRADOS/RECHAZADOS)
            // Usamos IgnoreQueryFilters() para ver registros con IsActive = false
            var existingApplication = await _context.Asesores
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.UsuarioId == userId);

            // CASO A: Ya existe el registro
            if (existingApplication != null)
            {
                // Si está activo, significa que ya tiene una solicitud pendiente o aprobada
                if (existingApplication.IsActive)
                {
                    return new GenericResponseDto { IsSuccess = false, Message = "Ya tienes una solicitud registrada y activa." };
                }

                // CASO B: Existe pero estaba borrado/rechazado (IsActive = false)
                // =============================================================
                // REACTIVAMOS LA SOLICITUD (UPDATE en lugar de INSERT)
                // =============================================================

                // Actualizamos los datos con lo nuevo que envió el usuario
                existingApplication.Especialidad = dto.Especialidad;
                existingApplication.Descripcion = dto.Descripcion;
                existingApplication.NivelEstudios = dto.NivelEstudios;
                existingApplication.InstitucionEducativa = dto.InstitucionEducativa;
                existingApplication.CampoEstudio = dto.CampoEstudio;
                existingApplication.AnioGraduacion = dto.AnioGraduacion;
                existingApplication.AniosExperiencia = dto.AniosExperiencia;
                existingApplication.ExperienciaLaboral = dto.ExperienciaLaboral;
                existingApplication.Certificaciones = dto.Certificaciones;
                existingApplication.DocumentoVerificacionUrl = dto.DocumentoUrl;

                // Reactivamos y reseteamos estado
                existingApplication.IsActive = true;
                existingApplication.EstaAprobado = false;
                existingApplication.ModifiedAt = DateTime.UtcNow;

                _context.Asesores.Update(existingApplication);
                await _context.SaveChangesAsync();

                return new GenericResponseDto { IsSuccess = true, Message = "Tu solicitud ha sido reactivada y actualizada." };
            }

            // CASO C: Es totalmente nuevo (INSERT)
            var newAsesor = new Asesor
            {
                UsuarioId = userId,
                Especialidad = dto.Especialidad,
                Descripcion = dto.Descripcion,
                NivelEstudios = dto.NivelEstudios,
                InstitucionEducativa = dto.InstitucionEducativa,
                CampoEstudio = dto.CampoEstudio,
                AnioGraduacion = dto.AnioGraduacion,
                AniosExperiencia = dto.AniosExperiencia,
                ExperienciaLaboral = dto.ExperienciaLaboral,
                Certificaciones = dto.Certificaciones,

                DocumentoVerificacionUrl = dto.DocumentoUrl,

                EstaAprobado = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Asesores.AddAsync(newAsesor);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Solicitud enviada correctamente." };
        }
    }
}