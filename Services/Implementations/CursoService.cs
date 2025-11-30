using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class CursoService : ICursoService
    {
        private readonly ApplicationDbContext _context;      // SQL Server (Datos principales)
        private readonly AnalyticsDbContext _analyticsDb;    // PostgreSQL (Ratings y Pagos)
        private readonly UserManager<Usuario> _userManager;

        public CursoService(ApplicationDbContext context, AnalyticsDbContext analyticsDb, UserManager<Usuario> userManager)
        {
            _context = context;
            _analyticsDb = analyticsDb;
            _userManager = userManager;
        }

        // --- 1. CREAR CURSO (Solo Asesor) ---
        public async Task<GenericResponseDto> CreateCursoAsync(CursoCreateDto dto, int asesorId)
        {
            // Validamos que el asesor exista y esté aprobado
            var asesor = await _context.Asesores.FindAsync(asesorId);
            if (asesor == null || !asesor.EstaAprobado)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permisos para crear cursos. Asegúrate de ser un asesor aprobado." };
            }

            var nuevoCurso = new Curso
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Costo = dto.Costo,
                AsesorId = asesorId,
                EstaPublicado = false, // Nace como borrador

                // Auditoría automática
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Cursos.AddAsync(nuevoCurso);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "¡Curso creado exitosamente! Ahora agrega lecciones." };
        }

        // --- 2. VER CATÁLOGO PÚBLICO (Con Ratings Reales) ---
        public async Task<IEnumerable<CursoPublicDto>> GetCursosPublicosAsync()
        {
            // A. Traer Cursos Activos de SQL Server
            var cursosSQL = await _context.Cursos
                .Where(c => c.EstaPublicado == true && c.IsActive == true)
                .Include(c => c.Asesor.Usuario) // Incluimos usuario para el nombre
                .ToListAsync();

            // B. Traer Ratings de PostgreSQL (Agrupados por Curso)
            // Esto evita el problema de consultas N+1
            var ratings = await _analyticsDb.Calificaciones
                .GroupBy(c => c.CursoId)
                .Select(g => new {
                    CursoId = g.Key,
                    Promedio = g.Average(c => c.Rating),
                    Total = g.Count()
                })
                .ToListAsync();

            // C. Combinar datos en memoria
            var resultado = cursosSQL.Select(c => {
                var stats = ratings.FirstOrDefault(r => r.CursoId == c.CursoId);
                return new CursoPublicDto
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Descripcion = c.Descripcion,
                    Costo = c.Costo,
                    EstaPublicado = c.EstaPublicado,
                    AsesorId = c.AsesorId, // int
                    AsesorNombre = c.Asesor.Usuario.UserName,

                    // Datos de PostgreSQL
                    PromedioCalificacion = stats?.Promedio ?? 0,
                    TotalCalificaciones = stats?.Total ?? 0
                };
            });

            return resultado;
        }

        // --- 3. PUBLICAR / OCULTAR CURSO (Toggle) ---
        public async Task<GenericResponseDto> PublishCursoAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);

            if (curso == null || !curso.IsActive)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "El curso no existe." };
            }

            // Verificación de propiedad
            if (curso.AsesorId != asesorId)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permiso para modificar este curso." };
            }

            // Lógica de Toggle (Si está público lo oculta, si está oculto lo publica)
            curso.EstaPublicado = !curso.EstaPublicado;

            await _context.SaveChangesAsync();

            string estado = curso.EstaPublicado ? "publicado" : "oculto";
            return new GenericResponseDto { IsSuccess = true, Message = $"Curso {estado} exitosamente." };
        }

        // --- 4. VER MIS CURSOS (Panel del Asesor) ---
        public async Task<IEnumerable<CursoPublicDto>> GetMyCursosAsync(int asesorId)
        {
            // A. Cursos del asesor en SQL
            var misCursos = await _context.Cursos
                .Where(c => c.AsesorId == asesorId && c.IsActive == true)
                .Include(c => c.Asesor.Usuario)
                .ToListAsync();

            if (!misCursos.Any()) return new List<CursoPublicDto>();

            // B. Ratings de esos cursos en Postgres
            var misIds = misCursos.Select(x => x.CursoId).ToList();
            var ratings = await _analyticsDb.Calificaciones
                .Where(c => misIds.Contains(c.CursoId))
                .GroupBy(c => c.CursoId)
                .Select(g => new {
                    CursoId = g.Key,
                    Promedio = g.Average(c => c.Rating),
                    Total = g.Count()
                })
                .ToListAsync();

            // C. Combinar
            var resultado = misCursos.Select(c => {
                var stats = ratings.FirstOrDefault(r => r.CursoId == c.CursoId);
                return new CursoPublicDto
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Descripcion = c.Descripcion,
                    Costo = c.Costo,
                    EstaPublicado = c.EstaPublicado,
                    AsesorId = c.AsesorId,
                    AsesorNombre = c.Asesor.Usuario.UserName,

                    PromedioCalificacion = stats?.Promedio ?? 0,
                    TotalCalificaciones = stats?.Total ?? 0
                };
            });

            return resultado;
        }

        // --- 5. ACTUALIZAR INFORMACIÓN DEL CURSO ---
        public async Task<GenericResponseDto> UpdateCursoAsync(int cursoId, CursoCreateDto dto, int asesorId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);

            if (curso == null || !curso.IsActive) return new GenericResponseDto { IsSuccess = false, Message = "Curso no encontrado." };

            if (curso.AsesorId != asesorId) return new GenericResponseDto { IsSuccess = false, Message = "No autorizado." };

            curso.Titulo = dto.Titulo;
            curso.Descripcion = dto.Descripcion;
            curso.Costo = dto.Costo;
            // ModifiedAt se actualiza solo por el interceptor

            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Curso actualizado." };
        }

        // --- 6. ELIMINAR CURSO (Soft Delete) ---
        public async Task<GenericResponseDto> DeleteCursoAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Lecciones) // Incluir lecciones para borrar en cascada lógica
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.IsActive);

            if (curso == null) return new GenericResponseDto { IsSuccess = false, Message = "Curso no encontrado." };

            if (curso.AsesorId != asesorId) return new GenericResponseDto { IsSuccess = false, Message = "No autorizado." };

            // Borrado lógico del curso
            _context.Cursos.Remove(curso);

            // Borrado lógico de sus lecciones
            foreach (var leccion in curso.Lecciones)
            {
                _context.Lecciones.Remove(leccion);
            }

            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Curso eliminado correctamente." };
        }

        // --- 7. OBTENER DETALLE PARA EDICIÓN (Asesor) ---
        public async Task<CursoPublicDto?> GetCursoByIdForAsesorAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Asesor.Usuario)
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.AsesorId == asesorId && c.IsActive);

            if (curso == null) return null;

            // Opcional: También podríamos traer los ratings aquí si los queremos mostrar en el editor
            // Por simplicidad y rendimiento en edición, los dejamos en 0 o hacemos una consulta rápida si es necesario.
            // Aquí los dejaré en 0 ya que es para "Gestión/Edición", no para "Visualización de Stats".

            return new CursoPublicDto
            {
                CursoId = curso.CursoId,
                Titulo = curso.Titulo,
                Descripcion = curso.Descripcion,
                Costo = curso.Costo,
                EstaPublicado = curso.EstaPublicado,
                AsesorId = curso.AsesorId,
                AsesorNombre = curso.Asesor.Usuario.UserName,
                PromedioCalificacion = 0, // No crítico para editar
                TotalCalificaciones = 0
            };
        }
    }
}