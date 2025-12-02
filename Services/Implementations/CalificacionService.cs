using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class CalificacionService : ICalificacionService
    {
        private readonly AnalyticsDbContext _analyticsDb;
        private readonly ApplicationDbContext _appDb;

        public CalificacionService(AnalyticsDbContext analyticsDb, ApplicationDbContext appDb)
        {
            _analyticsDb = analyticsDb;
            _appDb = appDb;
        }

        public async Task<GenericResponseDto> AddCalificacionAsync(int cursoId, int estudianteId, CalificacionCreateDto dto)
        {
            // 1. Verificamos inscripción y obtenemos el AsesorId del curso
            var inscripcion = await _appDb.Inscripciones
                .Include(i => i.Curso) // Traemos el curso para saber quién es el asesor
                .FirstOrDefaultAsync(i => i.CursoId == cursoId && i.EstudianteId == estudianteId && i.IsActive);

            if (inscripcion == null) return new GenericResponseDto { IsSuccess = false, Message = "No estás inscrito en este curso." };

            // 2. Verificar duplicado
            var yaCalifico = await _analyticsDb.Calificaciones
               .AnyAsync(c => c.CursoId == cursoId && c.EstudianteId == estudianteId);

            if (yaCalifico) return new GenericResponseDto { IsSuccess = false, Message = "Ya calificaste este curso." };

            // 3. Guardar con el nuevo campo AsesorId
            var nueva = new Calificacion
            {
                CursoId = cursoId,
                EstudianteId = estudianteId,
                AsesorId = inscripcion.Curso.AsesorId, // <--- ¡IMPORTANTE! Guardamos el ID del Asesor
                SolicitudId = null, // Es null porque es calificación de curso
                Rating = dto.Rating,
                Comentario = dto.Comentario,
                CreatedAt = DateTime.UtcNow
            };

            await _analyticsDb.Calificaciones.AddAsync(nueva);
            await _analyticsDb.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "¡Gracias por tu calificación!" };
        }

        public async Task<IEnumerable<CalificacionDetailDto>> GetCalificacionesCursoAsync(int cursoId, int asesorId)
        {
            var curso = await _appDb.Cursos
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.AsesorId == asesorId && c.IsActive);

            if (curso == null) return new List<CalificacionDetailDto>();

            var calificaciones = await _analyticsDb.Calificaciones
                .Where(c => c.CursoId == cursoId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            if (!calificaciones.Any()) return new List<CalificacionDetailDto>();

            var estudiantesIds = calificaciones.Select(c => c.EstudianteId).Distinct().ToList();

            var estudiantesInfo = await _appDb.Users
                .Where(u => estudiantesIds.Contains(u.Id))
                .Select(u => new { u.Id, Nombre = u.NombreCompleto ?? u.UserName })
                .ToDictionaryAsync(u => u.Id, u => u.Nombre);

            var resultado = calificaciones.Select(c => new CalificacionDetailDto
            {
                CalificacionId = c.CalificacionId,
                Rating = c.Rating,
                Comentario = c.Comentario,
                Fecha = c.CreatedAt,
                NombreEstudiante = estudiantesInfo.ContainsKey(c.EstudianteId) ? estudiantesInfo[c.EstudianteId] : "Estudiante"
            });

            return resultado;
        }
    }
}