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
        private readonly ApplicationDbContext _context;      // SQL Server
        private readonly AnalyticsDbContext _analyticsDb;    // PostgreSQL
        private readonly UserManager<Usuario> _userManager;

        public CursoService(ApplicationDbContext context, AnalyticsDbContext analyticsDb, UserManager<Usuario> userManager)
        {
            _context = context;
            _analyticsDb = analyticsDb;
            _userManager = userManager;
        }

        // --- 1. CREAR CURSO ---
        public async Task<GenericResponseDto> CreateCursoAsync(CursoCreateDto dto, int asesorId)
        {
            var asesor = await _context.Asesores.FindAsync(asesorId);
            if (asesor == null || !asesor.EstaAprobado)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permisos para crear cursos." };
            }

            var nuevoCurso = new Curso
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Costo = dto.Costo,
                AsesorId = asesorId,
                EstaPublicado = false,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Cursos.AddAsync(nuevoCurso);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Curso creado exitosamente." };
        }

        // --- 2. VER CATÁLOGO PÚBLICO ---
        public async Task<IEnumerable<CursoPublicDto>> GetCursosPublicosAsync()
        {
            // A. SQL Server
            var cursosSQL = await _context.Cursos
                .Where(c => c.EstaPublicado == true && c.IsActive == true)
                .Include(c => c.Asesor.Usuario)
                .ToListAsync();

            // B. PostgreSQL (Ratings)
            // CORRECCIÓN: Filtramos donde CursoId no sea nulo
            var ratings = await _analyticsDb.Calificaciones
                .Where(c => c.CursoId != null)
                .GroupBy(c => c.CursoId)
                .Select(g => new {
                    CursoId = g.Key,
                    Promedio = g.Average(c => c.Rating),
                    Total = g.Count()
                })
                .ToListAsync();

            // C. Combinar
            var resultado = cursosSQL.Select(c => {
                // Como g.Key es int?, comparamos con value o casteo seguro
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

        // --- 3. PUBLICAR / OCULTAR ---
        public async Task<GenericResponseDto> PublishCursoAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);

            if (curso == null || !curso.IsActive)
                return new GenericResponseDto { IsSuccess = false, Message = "El curso no existe." };

            if (curso.AsesorId != asesorId)
                return new GenericResponseDto { IsSuccess = false, Message = "No autorizado." };

            curso.EstaPublicado = !curso.EstaPublicado;
            await _context.SaveChangesAsync();

            string estado = curso.EstaPublicado ? "publicado" : "oculto";
            return new GenericResponseDto { IsSuccess = true, Message = $"Curso {estado} exitosamente." };
        }

        // --- 4. VER MIS CURSOS ---
        public async Task<IEnumerable<CursoPublicDto>> GetMyCursosAsync(int asesorId)
        {
            // A. SQL
            var misCursos = await _context.Cursos
                .Where(c => c.AsesorId == asesorId && c.IsActive == true)
                .Include(c => c.Asesor.Usuario)
                .ToListAsync();

            if (!misCursos.Any()) return new List<CursoPublicDto>();

            // B. Postgres
            var misIds = misCursos.Select(x => x.CursoId).ToList();

            // CORRECCIÓN AQUÍ: Usamos .Value para comparar int con int? de forma segura
            var ratings = await _analyticsDb.Calificaciones
                .Where(c => c.CursoId != null && misIds.Contains(c.CursoId.Value))
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

        // --- 5. ACTUALIZAR ---
        public async Task<GenericResponseDto> UpdateCursoAsync(int cursoId, CursoCreateDto dto, int asesorId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.IsActive) return new GenericResponseDto { IsSuccess = false, Message = "Curso no encontrado." };
            if (curso.AsesorId != asesorId) return new GenericResponseDto { IsSuccess = false, Message = "No autorizado." };

            curso.Titulo = dto.Titulo;
            curso.Descripcion = dto.Descripcion;
            curso.Costo = dto.Costo;
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Curso actualizado." };
        }

        // --- 6. ELIMINAR ---
        public async Task<GenericResponseDto> DeleteCursoAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Lecciones)
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.IsActive);

            if (curso == null) return new GenericResponseDto { IsSuccess = false, Message = "Curso no encontrado." };
            if (curso.AsesorId != asesorId) return new GenericResponseDto { IsSuccess = false, Message = "No autorizado." };

            _context.Cursos.Remove(curso);
            foreach (var leccion in curso.Lecciones)
            {
                _context.Lecciones.Remove(leccion);
            }
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Curso eliminado." };
        }

        // --- 7. DETALLE ---
        public async Task<CursoPublicDto?> GetCursoByIdForAsesorAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Asesor.Usuario)
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.AsesorId == asesorId && c.IsActive);

            if (curso == null) return null;

            return new CursoPublicDto
            {
                CursoId = curso.CursoId,
                Titulo = curso.Titulo,
                Descripcion = curso.Descripcion,
                Costo = curso.Costo,
                EstaPublicado = curso.EstaPublicado,
                AsesorId = curso.AsesorId,
                AsesorNombre = curso.Asesor.Usuario.UserName,
                PromedioCalificacion = 0,
                TotalCalificaciones = 0
            };
        }
    }
}