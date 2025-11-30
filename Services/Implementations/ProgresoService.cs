using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class ProgresoService : IProgresoService
    {
        private readonly ApplicationDbContext _context;

        public ProgresoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GenericResponseDto> MarcarLeccionAsync(int estudianteId, int leccionId, bool completada)
        {
            var progreso = await _context.Progresos
                .FirstOrDefaultAsync(p => p.EstudianteId == estudianteId && p.LeccionId == leccionId);

            if (progreso == null)
            {
                // Si no existe registro, lo creamos
                progreso = new ProgresoLeccion
                {
                    EstudianteId = estudianteId,
                    LeccionId = leccionId,
                    EstaCompletada = completada,
                    FechaCompletado = completada ? DateTime.UtcNow : null
                };
                await _context.Progresos.AddAsync(progreso);
            }
            else
            {
                // Si existe, lo actualizamos
                progreso.EstaCompletada = completada;
                progreso.FechaCompletado = completada ? DateTime.UtcNow : null;
                _context.Progresos.Update(progreso);
            }

            await _context.SaveChangesAsync();
            return new GenericResponseDto { IsSuccess = true, Message = completada ? "Lección completada." : "Lección desmarcada." };
        }

        public async Task<ProgresoCursoDto> ObtenerProgresoCursoAsync(int estudianteId, int cursoId)
        {
            // Obtener todas las lecciones del curso (para saber el total)
            var leccionesIds = await _context.Lecciones
                .Where(l => l.CursoId == cursoId && l.IsActive)
                .Select(l => l.LeccionId)
                .ToListAsync();

            if (!leccionesIds.Any())
                return new ProgresoCursoDto { CursoId = cursoId, LeccionesCompletadasIds = new List<int>() };

            // Obtener cuáles de esas están completadas por el estudiante
            var completadasIds = await _context.Progresos
                .Where(p => p.EstudianteId == estudianteId && leccionesIds.Contains(p.LeccionId) && p.EstaCompletada)
                .Select(p => p.LeccionId)
                .ToListAsync();

            var total = leccionesIds.Count;
            var completadas = completadasIds.Count;
            var porcentaje = total > 0 ? (double)completadas / total * 100 : 0;

            return new ProgresoCursoDto
            {
                CursoId = cursoId,
                LeccionesTotales = total,
                LeccionesCompletadas = completadas,
                Porcentaje = Math.Round(porcentaje, 1), // Redondear a 1 decimal
                LeccionesCompletadasIds = completadasIds
            };
        }
    }
}
