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
                    UsuarioId = a.UsuarioId,
                    UserName = a.Usuario.UserName ?? "Sin Usuario",
                    Email = a.Usuario.Email ?? "Sin Email",
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

        public async Task<GenericResponseDto> ReviewAsesorApplicationAsync(int userId, bool approve)
        {
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

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, "Asesor");
                }

                await _context.SaveChangesAsync();
                return new GenericResponseDto { IsSuccess = true, Message = "Asesor aprobado exitosamente." };
            }
            else
            {
                // Borrado lógico
                application.IsActive = false;
                _context.Asesores.Update(application);
                await _context.SaveChangesAsync();

                return new GenericResponseDto { IsSuccess = true, Message = "Solicitud rechazada." };
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
            // 1. TRAER LISTA BASE DE ASESORES Y SUS INGRESOS DE CURSOS (SQL Server + Postgres Linked)
            var query = @"
                SELECT
                    a.UsuarioId AS AsesorId,
                    u.UserName AS NombreAsesor,
                    COUNT(DISTINCT c.CursoId) AS TotalCursos,
                    
                    -- Traemos solo los INGRESOS de cursos desde Postgres
                    CAST(COALESCE(SUM(pg_pagos.IngresosGenerados), 0.0) AS DECIMAL(18,2)) AS IngresosCursos,

                    -- Placeholders para llenar después
                    0 AS TotalCalificaciones,
                    CAST(0 AS FLOAT) AS RatingPromedio,
                    CAST(0 AS DECIMAL(18,2)) AS IngresosAsesorias,
                    CAST(0 AS DECIMAL(18,2)) AS IngresosGenerados
                FROM
                    dbo.Asesores a
                JOIN
                    dbo.AspNetUsers u ON a.UsuarioId = u.Id
                LEFT JOIN
                    dbo.Cursos c ON a.UsuarioId = c.AsesorId AND c.IsActive = 1
                LEFT JOIN (
                    SELECT
                        CAST(""CursoId"" AS INT) AS CursoId,
                        SUM(""Monto"") AS IngresosGenerados
                    FROM OPENQUERY(POSTGRES_ANALYTICS, 
                        'SELECT ""CursoId"", ""Monto"" FROM ""public"".""HistorialDePagos""'
                    )
                    GROUP BY CAST(""CursoId"" AS INT)
                ) AS pg_pagos ON c.CursoId = pg_pagos.CursoId
                WHERE
                    a.EstaAprobado = 1 AND a.IsActive = 1
                GROUP BY
                    a.UsuarioId, u.UserName;
            ";

            var results = await _context.Database
                .SqlQueryRaw<AsesorRatingDto>(query)
                .ToListAsync();

            // 2. OBTENER RATING GLOBAL REAL (PostgreSQL - EF Core)
            var ratingsReales = await _analyticsDb.Calificaciones
                .GroupBy(c => c.AsesorId)
                .Select(g => new {
                    AsesorId = g.Key,
                    Promedio = g.Average(c => (double)c.Rating),
                    Total = g.Count()
                })
                .ToListAsync();

            // 3. OBTENER INGRESOS DE ASESORÍAS (SQL Server)
            var ingresosAsesorias = await _context.OfertasSolicitud
                .Where(o => o.FueAceptada && o.IsActive)
                .GroupBy(o => o.AsesorId)
                .Select(g => new { AsesorId = g.Key, Total = g.Sum(o => o.PrecioOferta) })
                .ToListAsync();

            // 4. MERGE FINAL EN MEMORIA
            foreach (var asesor in results)
            {
                // A. Asignar Rating Global Real
                var statsRating = ratingsReales.FirstOrDefault(x => x.AsesorId == asesor.AsesorId);
                if (statsRating != null)
                {
                    asesor.RatingPromedio = statsRating.Promedio;
                    asesor.TotalCalificaciones = statsRating.Total;
                }
                else
                {
                    asesor.RatingPromedio = 0;
                    asesor.TotalCalificaciones = 0;
                }

                // B. Sumar Ingresos
                var extraIncome = ingresosAsesorias.FirstOrDefault(x => x.AsesorId == asesor.AsesorId);
                asesor.IngresosAsesorias = extraIncome?.Total ?? 0;
                asesor.IngresosGenerados = asesor.IngresosCursos + asesor.IngresosAsesorias;
            }

            return results.OrderByDescending(a => a.IngresosGenerados).ToList();
        }

        public async Task<IEnumerable<MonthlyStatsDto>> GetMonthlyRevenueAsync()
        {
            var anioActual = DateTime.UtcNow.Year;

            // 1. Ingresos Cursos (PostgreSQL)
            var pagosCursos = await _analyticsDb.HistorialDePagos
                .Where(p => p.FechaPago.Year == anioActual)
                .GroupBy(p => p.FechaPago.Month)
                .Select(g => new { Mes = g.Key, Total = g.Sum(x => x.Monto) })
                .ToListAsync();

            // 2. Ingresos Asesorías (SQL Server)
            var pagosAsesorias = await _context.OfertasSolicitud
                .Where(o => o.FueAceptada && o.IsActive && o.ModifiedAt.Year == anioActual)
                .GroupBy(o => o.ModifiedAt.Month)
                .Select(g => new { Mes = g.Key, Total = g.Sum(x => x.PrecioOferta) })
                .ToListAsync();

            var resultado = new List<MonthlyStatsDto>();
            string[] nombresMeses = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            for (int i = 1; i <= 12; i++)
            {
                var datoCurso = pagosCursos.FirstOrDefault(p => p.Mes == i);
                var datoAsesoria = pagosAsesorias.FirstOrDefault(p => p.Mes == i);

                resultado.Add(new MonthlyStatsDto
                {
                    Mes = nombresMeses[i - 1],
                    IngresosCursos = datoCurso?.Total ?? 0,
                    IngresosAsesorias = datoAsesoria?.Total ?? 0
                });
            }

            return resultado;
        }

        // --- AQUÍ ESTÁ EL CAMBIO IMPORTANTE: AsSplitQuery() ---
        public async Task<AsesorDetailFullDto> GetAsesorDetailsAsync(int asesorId)
        {
            var asesor = await _context.Asesores
                .Include(a => a.Usuario)
                .Include(a => a.Cursos).ThenInclude(c => c.Inscripciones)
                .Include(a => a.SolicitudesAtendidas).ThenInclude(s => s.Estudiante)
                .Include(a => a.SolicitudesAtendidas).ThenInclude(s => s.Ofertas)
                .AsSplitQuery() // <--- ESTO OPTIMIZA LA CONSULTA Y EVITA LA LENTITUD
                .FirstOrDefaultAsync(a => a.UsuarioId == asesorId);

            if (asesor == null) return null!;

            // 1. Ingresos Cursos (Postgres)
            var cursoIds = asesor.Cursos.Select(c => c.CursoId).ToList();
            decimal ingresosCursos = 0;
            if (cursoIds.Any())
            {
                ingresosCursos = await _analyticsDb.HistorialDePagos
                    .Where(p => cursoIds.Contains(p.CursoId))
                    .SumAsync(p => p.Monto);
            }

            // 2. Ingresos Asesorías (SQL Server)
            var ingresosAsesorias = asesor.SolicitudesAtendidas
                .SelectMany(s => s.Ofertas)
                .Where(o => o.AsesorId == asesorId && o.FueAceptada)
                .Sum(o => o.PrecioOferta);

            // 3. Rating Global
            var rating = 0.0;

            var ratingsQuery = await _analyticsDb.Calificaciones
                 .Where(c => c.AsesorId == asesorId)
                 .Select(c => c.Rating)
                 .ToListAsync();

            if (ratingsQuery.Any()) rating = ratingsQuery.Average();

            return new AsesorDetailFullDto
            {
                NombreCompleto = asesor.Usuario.NombreCompleto ?? asesor.Usuario.UserName ?? "Asesor",
                Email = asesor.Usuario.Email ?? "Sin Email",
                Telefono = asesor.Usuario.PhoneNumber ?? "N/A",
                Especialidad = asesor.Especialidad,

                TotalCursos = asesor.Cursos.Count,
                TotalAsesorias = asesor.SolicitudesAtendidas.Count,
                TotalEstudiantes = asesor.Cursos.Sum(c => c.Inscripciones.Count) + asesor.SolicitudesAtendidas.Count,
                TotalIngresos = ingresosCursos + ingresosAsesorias,
                RatingPromedio = rating,

                Cursos = asesor.Cursos.Select(c => new CursoSimpleDto
                {
                    Titulo = c.Titulo,
                    Costo = c.Costo,
                    Estado = c.EstaPublicado,
                    Inscritos = c.Inscripciones.Count
                }).ToList(),

                Asesorias = asesor.SolicitudesAtendidas.Select(s => {
                    var oferta = s.Ofertas.FirstOrDefault(o => o.FueAceptada);
                    return new AsesoriaSimpleDto
                    {
                        Tema = s.Tema,
                        Materia = s.Materia,
                        Precio = oferta?.PrecioOferta ?? 0,
                        Fecha = s.ModifiedAt,
                        Estudiante = s.Estudiante?.UserName ?? "Usuario"
                    };
                }).ToList()
            };
        }

        public async Task<AdminAsesoriasStatsDto> GetAsesoriasGlobalStatsAsync(int page = 1, int pageSize = 10)
        {
            // 1. Total de registros (Para calcular páginas en el frontend)
            var total = await _context.SolicitudesDeAyuda
                .CountAsync(s => (s.Estado == "EnProceso" || s.Estado == "Finalizada") && s.IsActive);

            // 2. Ingresos Totales (Esta métrica siempre es global, no cambia por página)
            var ingresos = await _context.OfertasSolicitud
                .Where(o => o.FueAceptada && o.IsActive)
                .SumAsync(o => o.PrecioOferta);

            // 3. Obtener solo los registros de la página actual
            var ultimas = await _context.SolicitudesDeAyuda
                .Where(s => (s.Estado == "EnProceso" || s.Estado == "Finalizada") && s.IsActive)
                .Include(s => s.Estudiante)
                .Include(s => s.Ofertas)
                .OrderByDescending(s => s.ModifiedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var listaDtos = ultimas.Select(s => {
                var oferta = s.Ofertas.FirstOrDefault(o => o.FueAceptada);
                return new AsesorJobsDto
                {
                    SolicitudId = s.SolicitudId,
                    Materia = s.Materia,
                    Tema = s.Tema,
                    Descripcion = s.Descripcion,
                    FechaLimite = s.FechaLimite,
                    ArchivoUrl = s.ArchivoUrl ?? "",
                    Precio = oferta?.PrecioOferta ?? 0,
                    NombreEstudiante = s.Estudiante?.NombreCompleto ?? s.Estudiante?.UserName ?? "Usuario",
                    EmailEstudiante = s.Estudiante?.Email ?? "",
                    FechaAceptacion = s.ModifiedAt
                };
            }).ToList();

            return new AdminAsesoriasStatsDto
            {
                TotalAsesoriasCerradas = total,
                IngresosTotalesAsesorias = ingresos,
                UltimasAsesorias = listaDtos
            };
        }
    }
}