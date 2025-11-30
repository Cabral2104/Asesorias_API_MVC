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

        public async Task<GenericResponseDto> ApplyToBeAsesorAsync(AsesorApplyDto dto, int userId)
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

        public async Task<IEnumerable<ChartStatDto>> GetActivityChartAsync(int asesorId)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-6).Date; // Últimos 7 días incluyendo hoy

            // 1. Obtener inscripciones crudas de la BD (Filtradas por fecha y asesor)
            // Hacemos el filtro en BD, pero el agrupamiento en memoria para evitar errores de traducción de fechas en EF Core
            var inscripcionesRecientes = await _context.Inscripciones
                .Include(i => i.Curso)
                .Where(i => i.Curso.AsesorId == asesorId && i.CreatedAt >= fechaLimite && i.IsActive)
                .Select(i => i.CreatedAt)
                .ToListAsync();

            // 2. Agrupar en Memoria
            var actividad = inscripcionesRecientes
                .GroupBy(fecha => fecha.ToLocalTime().Date)
                .Select(g => new { Fecha = g.Key, Total = g.Count() })
                .ToList();

            // 3. Rellenar los días vacíos (para que la gráfica se vea continua)
            var resultado = new List<ChartStatDto>();
            var diasSemana = new string[] { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };

            for (int i = 0; i < 7; i++)
            {
                var fechaActual = fechaLimite.AddDays(i);
                var datoEncontrado = actividad.FirstOrDefault(a => a.Fecha == fechaActual);

                resultado.Add(new ChartStatDto
                {
                    Dia = diasSemana[(int)fechaActual.DayOfWeek], // Obtiene el nombre del día
                    Cantidad = datoEncontrado?.Total ?? 0
                });
            }

            return resultado;
        }
    }
}