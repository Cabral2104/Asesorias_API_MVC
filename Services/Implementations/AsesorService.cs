using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class AsesorService : IAsesorService
    {
        private readonly ApplicationDbContext _context;      // SQL Server
        private readonly AnalyticsDbContext _analyticsDb;    // PostgreSQL (Para ratings y pagos)

        public AsesorService(ApplicationDbContext context, AnalyticsDbContext analyticsDb)
        {
            _context = context;
            _analyticsDb = analyticsDb;
        }

        // --- 1. SOLICITAR SER ASESOR (Crear o Reactivar) ---
        public async Task<GenericResponseDto> ApplyToBeAsesorAsync(AsesorApplyDto dto, int userId)
        {
            // Buscar si ya existe (incluso si fue borrado/rechazado antes)
            var existingApplication = await _context.Asesores
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.UsuarioId == userId);

            if (existingApplication != null)
            {
                // Si está activo, ya tiene una solicitud pendiente o aprobada
                if (existingApplication.IsActive)
                {
                    return new GenericResponseDto { IsSuccess = false, Message = "Ya tienes una solicitud registrada y activa." };
                }

                // Si estaba inactivo (rechazado o borrado), reactivamos y actualizamos
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

                existingApplication.IsActive = true;
                existingApplication.EstaAprobado = false; // Reiniciamos aprobación para nueva revisión
                existingApplication.ModifiedAt = DateTime.UtcNow;

                _context.Asesores.Update(existingApplication);
                await _context.SaveChangesAsync();

                return new GenericResponseDto { IsSuccess = true, Message = "Tu solicitud ha sido reactivada y actualizada." };
            }

            // Si es nuevo, creamos el registro
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

        // --- 2. GRÁFICA DE ACTIVIDAD (Últimos 7 días) ---
        public async Task<IEnumerable<ChartStatDto>> GetActivityChartAsync(int asesorId)
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-6).Date;

            // Obtener inscripciones crudas de la BD
            var inscripcionesRecientes = await _context.Inscripciones
                .Include(i => i.Curso)
                .Where(i => i.Curso.AsesorId == asesorId && i.CreatedAt >= fechaLimite && i.IsActive)
                .Select(i => i.CreatedAt)
                .ToListAsync();

            // Agrupar en Memoria
            var actividad = inscripcionesRecientes
                .GroupBy(fecha => fecha.ToLocalTime().Date)
                .Select(g => new { Fecha = g.Key, Total = g.Count() })
                .ToList();

            // Rellenar días vacíos
            var resultado = new List<ChartStatDto>();
            var diasSemana = new string[] { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };

            for (int i = 0; i < 7; i++)
            {
                var fechaActual = fechaLimite.AddDays(i);
                var datoEncontrado = actividad.FirstOrDefault(a => a.Fecha == fechaActual);

                resultado.Add(new ChartStatDto
                {
                    Dia = diasSemana[(int)fechaActual.DayOfWeek],
                    Cantidad = datoEncontrado?.Total ?? 0
                });
            }

            return resultado;
        }

        // --- 3. MIS TRABAJOS (Asesorías Personalizadas) ---
        public async Task<IEnumerable<AsesorJobsDto>> GetMyJobsAsync(int asesorId)
        {
            // 1. Obtener trabajos de SQL Server
            var trabajos = await _context.SolicitudesDeAyuda
                .Where(s => s.AsesorAsignadoId == asesorId && (s.Estado == "EnProceso" || s.Estado == "Finalizada") && s.IsActive)
                .Include(s => s.Estudiante)
                .Include(s => s.Ofertas)
                .OrderByDescending(s => s.ModifiedAt)
                .ToListAsync();

            if (!trabajos.Any()) return new List<AsesorJobsDto>();

            // 2. Obtener calificaciones de estos trabajos desde PostgreSQL
            var solicitudIds = trabajos.Select(t => t.SolicitudId).ToList();

            var ratings = await _analyticsDb.Calificaciones
                .Where(c => c.SolicitudId != null && solicitudIds.Contains(c.SolicitudId.Value))
                .ToListAsync();

            // 3. Mapear y Unir
            return trabajos.Select(s => {
                var ofertaGanadora = s.Ofertas.FirstOrDefault(o => o.AsesorId == asesorId && o.FueAceptada);
                var feedback = ratings.FirstOrDefault(r => r.SolicitudId == s.SolicitudId);

                return new AsesorJobsDto
                {
                    SolicitudId = s.SolicitudId,
                    Materia = s.Materia,
                    Tema = s.Tema,
                    Descripcion = s.Descripcion,
                    FechaLimite = s.FechaLimite,
                    ArchivoUrl = s.ArchivoUrl,
                    Precio = ofertaGanadora?.PrecioOferta ?? 0,
                    NombreEstudiante = s.Estudiante.NombreCompleto ?? s.Estudiante.UserName,
                    EmailEstudiante = s.Estudiante.Email,
                    FechaAceptacion = s.ModifiedAt,

                    // Feedback
                    Rating = feedback?.Rating,
                    Comentario = feedback?.Comentario
                };
            });
        }

        // --- 4. PERFIL PÚBLICO DEL ASESOR ---
        public async Task<AsesorPublicProfileDto?> GetPublicProfileAsync(int asesorId)
        {
            var asesor = await _context.Asesores
                .Include(a => a.Usuario)
                .Include(a => a.Cursos).ThenInclude(c => c.Inscripciones)
                .Include(a => a.SolicitudesAtendidas) // Incluimos asesorías para contar estudiantes totales
                .FirstOrDefaultAsync(a => a.UsuarioId == asesorId && a.IsActive);

            if (asesor == null) return null;

            // Obtener cursos públicos del asesor
            var cursosPublicos = asesor.Cursos
                .Where(c => c.EstaPublicado && c.IsActive)
                .Select(c => new CursoPublicDto
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Descripcion = c.Descripcion,
                    Costo = c.Costo,
                    EstaPublicado = c.EstaPublicado,
                    AsesorId = c.AsesorId,
                    AsesorNombre = asesor.Usuario.NombreCompleto ?? asesor.Usuario.UserName
                    // Podríamos calcular ratings individuales aquí si se requiere
                }).ToList();

            // Calcular Rating Global Real desde PostgreSQL (Cursos + Asesorías)
            // Usamos el campo AsesorId que agregamos a la tabla Calificaciones
            double rating = 0;
            var ratingsQuery = await _analyticsDb.Calificaciones
                .Where(r => r.AsesorId == asesorId)
                .Select(r => r.Rating)
                .ToListAsync();

            if (ratingsQuery.Any())
            {
                rating = ratingsQuery.Average();
            }

            // Total de estudiantes = Inscritos en Cursos + Clientes de Asesorías
            int totalEstudiantes = asesor.Cursos.Sum(c => c.Inscripciones.Count) + asesor.SolicitudesAtendidas.Count;

            return new AsesorPublicProfileDto
            {
                AsesorId = asesor.UsuarioId,
                Nombre = asesor.Usuario.NombreCompleto ?? asesor.Usuario.UserName,
                Especialidad = asesor.Especialidad,
                Descripcion = asesor.Descripcion,
                NivelEstudios = asesor.NivelEstudios,
                AniosExperiencia = asesor.AniosExperiencia,
                RatingPromedio = rating,
                TotalEstudiantes = totalEstudiantes,
                Cursos = cursosPublicos
            };
        }
    }
}