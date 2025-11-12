using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class CalificacionService : ICalificacionService
    {
        // ¡Inyectamos AMBOS DbContexts!
        private readonly AnalyticsDbContext _analyticsDb; // PostgreSQL
        private readonly ApplicationDbContext _appDb;       // SQL Server

        public CalificacionService(AnalyticsDbContext analyticsDb, ApplicationDbContext appDb)
        {
            _analyticsDb = analyticsDb;
            _appDb = appDb;
        }

        public async Task<GenericResponseDto> AddCalificacionAsync(int cursoId, string estudianteId, CalificacionCreateDto dto)
        {
            // 1. Verificamos en SQL Server que el estudiante esté inscrito
            var estaInscrito = await _appDb.Inscripciones
                .AnyAsync(i => i.CursoId == cursoId && i.EstudianteId == estudianteId && i.IsActive);

            if (!estaInscrito)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No puedes calificar un curso en el que no estás inscrito." };
            }

            // 2. Verificamos en PostgreSQL que no haya calificado ya
            var yaCalifico = await _analyticsDb.Calificaciones
                .AnyAsync(c => c.CursoId == cursoId && c.EstudianteId == estudianteId);

            if (yaCalifico)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Ya has calificado este curso." };
            }

            // 3. Creamos la calificación
            var nuevaCalificacion = new Calificacion
            {
                CursoId = cursoId,
                EstudianteId = estudianteId,
                Rating = dto.Rating,
                Comentario = dto.Comentario,
                CreatedAt = DateTime.UtcNow
            };

            // 4. Guardamos en PostgreSQL
            await _analyticsDb.Calificaciones.AddAsync(nuevaCalificacion);
            await _analyticsDb.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "¡Gracias por tu calificación!" };
        }
    }
}
