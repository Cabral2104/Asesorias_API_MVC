using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly AnalyticsDbContext _analyticsDb;

        public AdminService(ApplicationDbContext context, UserManager<Usuario> userManager, AnalyticsDbContext analyticsDb)
        {
            _context = context;
            _userManager = userManager;
            _analyticsDb = analyticsDb;
        }

        public async Task<IEnumerable<SolicitudAsesorDto>> GetPendingAsesorApplicationsAsync()
        {
            var pendingApplications = await _context.Asesores
                .Where(a => a.EstaAprobado == false && a.IsActive == true)
                .Include(a => a.Usuario)
                .Select(a => new SolicitudAsesorDto
                {
                    UsuarioId = a.UsuarioId, // Ahora es int
                    UserName = a.Usuario.UserName,
                    Email = a.Usuario.Email,
                    Especialidad = a.Especialidad,
                    Descripcion = a.Descripcion,
                    NivelEstudios = a.NivelEstudios,
                    InstitucionEducativa = a.InstitucionEducativa,
                    CampoEstudio = a.CampoEstudio,
                    AnioGraduacion = a.AnioGraduacion,
                    AniosExperiencia = a.AniosExperiencia,
                    ExperienciaLaboral = a.ExperienciaLaboral,
                    Certificaciones = a.Certificaciones,
                    DocumentoVerificacionUrl = a.DocumentoVerificacionUrl,
                    FechaSolicitud = a.CreatedAt
                })
                .ToListAsync();

            return pendingApplications;
        }

        // --- CORRECCIÓN AQUÍ: int userId ---
        public async Task<GenericResponseDto> ReviewAsesorApplicationAsync(int userId, bool approve)
        {
            // FindAsync funciona directo con int porque la PK de Asesores ahora es int
            var application = await _context.Asesores.FindAsync(userId);

            if (application == null || application.IsActive == false)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Solicitud no encontrada." };
            }

            if (application.EstaAprobado)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Esta solicitud ya fue aprobada." };
            }

            if (approve)
            {
                application.EstaAprobado = true;

                // UserManager siempre espera string, así que convertimos el int
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, "Asesor");
                }
                await _context.SaveChangesAsync();
                return new GenericResponseDto { IsSuccess = true, Message = "Asesor aprobado exitosamente. Rol asignado." };
            }
            else
            {
                _context.Asesores.Remove(application);
                await _context.SaveChangesAsync();
                return new GenericResponseDto { IsSuccess = true, Message = "Solicitud rechazada y archivada." };
            }
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var totalUsuarios = await _context.Users.CountAsync(u => u.IsActive);
            var totalAsesores = await _context.Asesores.CountAsync(a => a.EstaAprobado && a.IsActive);
            var totalCursos = await _context.Cursos.CountAsync(c => c.EstaPublicado && c.IsActive);

            var ingresosTotales = await _analyticsDb.HistorialDePagos.SumAsync(p => p.Monto);
            var calificacionesTotales = await _analyticsDb.Calificaciones.CountAsync();

            double ratingPromedioGlobal = 0;
            if (calificacionesTotales > 0)
            {
                ratingPromedioGlobal = await _analyticsDb.Calificaciones.AverageAsync(c => c.Rating);
            }

            return new DashboardStatsDto
            {
                TotalUsuarios = totalUsuarios,
                TotalAsesoresAprobados = totalAsesores,
                TotalCursosPublicados = totalCursos,
                IngresosTotales = ingresosTotales,
                CalificacionesTotales = calificacionesTotales,
                RatingPromedioGlobal = ratingPromedioGlobal
            };
        }

        public async Task<IEnumerable<AsesorRatingDto>> GetAsesorDashboardAsync()
        {
            var query = @"
                SELECT
                    a.UsuarioId AS AsesorId,
                    u.UserName AS NombreAsesor,
                    COUNT(DISTINCT c.CursoId) AS TotalCursos,
                    COALESCE(SUM(pg_stats.TotalCalificaciones), 0) AS TotalCalificaciones,
                    COALESCE(AVG(pg_stats.RatingPromedio), 0.0) AS RatingPromedio,
                    COALESCE(SUM(pg_stats.IngresosGenerados), 0.0) AS IngresosGenerados
                FROM
                    dbo.Asesores a
                JOIN
                    dbo.AspNetUsers u ON a.UsuarioId = u.Id
                LEFT JOIN
                    dbo.Cursos c ON a.UsuarioId = c.AsesorId AND c.IsActive = 1
                LEFT JOIN (
                    SELECT
                        CAST(""CursoId"" AS INT) AS CursoId,
                        COUNT(*) AS TotalCalificaciones,
                        AVG(CAST(""Rating"" AS FLOAT)) AS RatingPromedio,
                        SUM(""Monto"" ) AS IngresosGenerados
                    FROM OPENQUERY(POSTGRES_ANALYTICS, 
                        'SELECT c.""CursoId"", c.""Rating"", p.""Monto""
                         FROM ""public"".""Calificaciones"" c
                         JOIN ""public"".""HistorialDePagos"" p ON c.""CursoId"" = p.""CursoId""'
                    ) AS pg_data
                    GROUP BY CAST(""CursoId"" AS INT)
                ) AS pg_stats ON c.CursoId = pg_stats.CursoId
                WHERE
                    a.EstaAprobado = 1 AND a.IsActive = 1
                GROUP BY
                    a.UsuarioId, u.UserName
                ORDER BY
                    IngresosGenerados DESC, RatingPromedio DESC;
            ";

            var results = await _context.Database
                .SqlQueryRaw<AsesorRatingDto>(query)
                .ToListAsync();

            return results;
        }

        public async Task<IEnumerable<MonthlyStatsDto>> GetMonthlyRevenueAsync()
        {
            var anioActual = DateTime.UtcNow.Year;

            var pagosPorMes = await _analyticsDb.HistorialDePagos
                .Where(p => p.FechaPago.Year == anioActual)
                .GroupBy(p => p.FechaPago.Month)
                .Select(g => new { Mes = g.Key, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            var resultado = new List<MonthlyStatsDto>();
            string[] nombresMeses = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            for (int i = 1; i <= 12; i++)
            {
                var datosMes = pagosPorMes.FirstOrDefault(p => p.Mes == i);
                resultado.Add(new MonthlyStatsDto
                {
                    Mes = nombresMeses[i - 1],
                    Ingresos = datosMes?.Total ?? 0
                });
            }

            return resultado;
        }

        // --- CORRECCIÓN: int asesorId ---
        public async Task<AsesorDetailFullDto> GetAsesorDetailsAsync(int asesorId)
        {
            var asesor = await _context.Asesores
                .Include(a => a.Usuario)
                .Include(a => a.Cursos)
                    .ThenInclude(c => c.Inscripciones)
                .FirstOrDefaultAsync(a => a.UsuarioId == asesorId);

            if (asesor == null) return null;

            var cursoIds = asesor.Cursos.Select(c => c.CursoId).ToList();

            var stats = await _analyticsDb.HistorialDePagos
                .Where(p => cursoIds.Contains(p.CursoId))
                .SumAsync(p => p.Monto);

            var rating = 0.0;
            var countRatings = await _analyticsDb.Calificaciones
                .Where(c => cursoIds.Contains(c.CursoId))
                .CountAsync();

            if (countRatings > 0)
            {
                rating = await _analyticsDb.Calificaciones
                    .Where(c => cursoIds.Contains(c.CursoId))
                    .AverageAsync(c => c.Rating);
            }

            return new AsesorDetailFullDto
            {
                NombreCompleto = asesor.Usuario.NombreCompleto ?? asesor.Usuario.UserName,
                Email = asesor.Usuario.Email,
                Telefono = asesor.Usuario.PhoneNumber ?? "N/A",
                Especialidad = asesor.Especialidad,
                TotalCursos = asesor.Cursos.Count,
                TotalEstudiantes = asesor.Cursos.Sum(c => c.Inscripciones.Count),
                TotalIngresos = stats,
                RatingPromedio = rating,
                Cursos = asesor.Cursos.Select(c => new CursoSimpleDto
                {
                    Titulo = c.Titulo,
                    Costo = c.Costo,
                    Estado = c.EstaPublicado,
                    Inscritos = c.Inscripciones.Count
                }).ToList()
            };
        }
    }
}