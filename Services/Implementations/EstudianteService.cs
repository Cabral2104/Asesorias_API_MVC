using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class EstudianteService : IEstudianteService
    {
        private readonly ApplicationDbContext _context;

        public EstudianteService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- LÓGICA PARA INSCRIBIRSE A UN CURSO ---
        public async Task<GenericResponseDto> InscribirseACursoAsync(int cursoId, string estudianteId)
        {
            // 1. Verificar que el curso existe, está publicado y activo
            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.EstaPublicado && c.IsActive);

            if (curso == null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Este curso no existe o no está disponible." };
            }

            // 2. Verificar que el estudiante no esté ya inscrito
            var yaInscrito = await _context.Inscripciones
                .AnyAsync(i => i.CursoId == cursoId && i.EstudianteId == estudianteId && i.IsActive);

            if (yaInscrito)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Ya estás inscrito en este curso." };
            }

            // 3. Crear la inscripción
            var nuevaInscripcion = new Inscripcion
            {
                EstudianteId = estudianteId,
                CursoId = cursoId
                // CreatedAt, ModifiedAt, IsActive se manejan automáticamente por el DbContext
            };

            await _context.Inscripciones.AddAsync(nuevaInscripcion);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "¡Inscripción exitosa!" };
        }

        // --- LÓGICA PARA VER "MIS CURSOS" ---
        public async Task<IEnumerable<CursoPublicDto>> GetMisCursosAsync(string estudianteId)
        {
            var misInscripciones = await _context.Inscripciones
                .Where(i => i.EstudianteId == estudianteId && i.IsActive)
                .Include(i => i.Curso) // Carga el Curso
                    .ThenInclude(c => c.Asesor) // Carga el Asesor del Curso
                    .ThenInclude(a => a.Usuario) // Carga el Usuario del Asesor
                .Select(i => new CursoPublicDto // Usamos el DTO público que ya teníamos
                {
                    CursoId = i.Curso.CursoId,
                    Titulo = i.Curso.Titulo,
                    Descripcion = i.Curso.Descripcion,
                    Costo = i.Curso.Costo,
                    EstaPublicado = i.Curso.EstaPublicado,
                    AsesorId = i.Curso.AsesorId,
                    AsesorNombre = i.Curso.Asesor.Usuario.UserName
                })
                .ToListAsync();

            return misInscripciones;
        }
    }
}
