using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class LeccionService : ILeccionService
    {
        private readonly ApplicationDbContext _context;

        public LeccionService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- OBTENER LECCIONES (Público) ---
        public async Task<IEnumerable<LeccionPublicDto>> GetLeccionesByCursoIdAsync(int cursoId)
        {
            // Verificamos que el curso exista y esté publicado
            var curso = await _context.Cursos
                .AsNoTracking() // Mejora el rendimiento, solo es lectura
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.EstaPublicado && c.IsActive);

            if (curso == null)
            {
                // Si el curso no existe o no está público, regresamos una lista vacía
                return new List<LeccionPublicDto>();
            }

            return await _context.Lecciones
                .Where(l => l.CursoId == cursoId && l.IsActive)
                .OrderBy(l => l.Orden)
                .Select(l => new LeccionPublicDto
                {
                    LeccionId = l.LeccionId,
                    Titulo = l.Titulo,
                    ContenidoUrl = l.ContenidoUrl,
                    Orden = l.Orden
                })
                .ToListAsync();
        }

        // --- AGREGAR LECCIÓN (Solo Asesor Dueño) ---
        public async Task<GenericResponseDto> AddLeccionToCursoAsync(int cursoId, LeccionCreateDto dto, string asesorId)
        {
            // Verificamos que el curso exista Y que le pertenezca al asesor
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.AsesorId == asesorId && c.IsActive);

            if (curso == null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No se encontró el curso o no tienes permisos para editarlo." };
            }

            var nuevaLeccion = new Leccion
            {
                Titulo = dto.Titulo,
                ContenidoUrl = dto.ContenidoUrl,
                Orden = dto.Orden,
                CursoId = cursoId // Asignamos al curso verificado
            };

            await _context.Lecciones.AddAsync(nuevaLeccion);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Lección agregada exitosamente." };
        }

        // --- ACTUALIZAR LECCIÓN (Solo Asesor Dueño) ---
        public async Task<GenericResponseDto> UpdateLeccionAsync(int leccionId, LeccionCreateDto dto, string asesorId)
        {
            // Para verificar propiedad, debemos unir Leccion -> Curso
            var leccion = await _context.Lecciones
                .Include(l => l.Curso) // Cargamos el Curso padre
                .FirstOrDefaultAsync(l => l.LeccionId == leccionId && l.IsActive);

            if (leccion == null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Lección no encontrada." };
            }

            // ¡Verificación de propiedad!
            if (leccion.Curso.AsesorId != asesorId)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permisos para editar esta lección." };
            }

            // Actualizamos los campos
            leccion.Titulo = dto.Titulo;
            leccion.ContenidoUrl = dto.ContenidoUrl;
            leccion.Orden = dto.Orden;
            // ModifiedAt se actualiza solo

            await _context.SaveChangesAsync();
            return new GenericResponseDto { IsSuccess = true, Message = "Lección actualizada." };
        }

        // --- ELIMINAR LECCIÓN (Solo Asesor Dueño) ---
        public async Task<GenericResponseDto> DeleteLeccionAsync(int leccionId, string asesorId)
        {
            var leccion = await _context.Lecciones
                .Include(l => l.Curso)
                .FirstOrDefaultAsync(l => l.LeccionId == leccionId && l.IsActive);

            if (leccion == null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Lección no encontrada." };
            }

            // ¡Verificación de propiedad!
            if (leccion.Curso.AsesorId != asesorId)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permisos para eliminar esta lección." };
            }

            // Borrado lógico (nuestro DbContext lo intercepta)
            _context.Lecciones.Remove(leccion);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Lección eliminada." };
        }
    }
}
