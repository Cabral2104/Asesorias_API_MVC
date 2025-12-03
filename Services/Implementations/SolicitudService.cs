using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class SolicitudService : ISolicitudService
    {
        private readonly ApplicationDbContext _context; // SQL
        private readonly AnalyticsDbContext _analyticsDb; // Postgres (Auditoría)
        private readonly IEmailService _emailService;

        public SolicitudService(ApplicationDbContext context, AnalyticsDbContext analyticsDb, IEmailService emailService)
        {
            _context = context;
            _analyticsDb = analyticsDb;
            _emailService = emailService;
        }

        // 1. CREAR SOLICITUD
        public async Task<GenericResponseDto> CrearSolicitudAsync(SolicitudCreateDto dto, int estudianteId)
        {
            var solicitud = new SolicitudDeAyuda
            {
                EstudianteId = estudianteId,
                Materia = dto.Materia,      // Nuevo
                Tema = dto.Tema,
                Descripcion = dto.Descripcion,
                FechaLimite = dto.FechaLimite, // Nuevo
                ArchivoUrl = dto.ArchivoUrl,   // Nuevo
                Estado = "Abierta"
            };

            await _context.SolicitudesDeAyuda.AddAsync(solicitud);
            await _context.SaveChangesAsync();

            // --- AUDITORÍA EN POSTGRES ---
            await RegistrarAuditoria(estudianteId, "Crear Solicitud", $"Tema: {dto.Tema}, Materia: {dto.Materia}");

            return new GenericResponseDto { IsSuccess = true, Message = "Solicitud publicada correctamente." };
        }

        // 2. VER MIS SOLICITUDES (Estudiante)
        public async Task<IEnumerable<SolicitudDetailDto>> GetMisSolicitudesAsync(int estudianteId)
        {
            return await _context.SolicitudesDeAyuda
                .Where(s => s.EstudianteId == estudianteId && s.IsActive)
                .Include(s => s.Estudiante)
                .Include(s => s.Ofertas).ThenInclude(o => o.Asesor).ThenInclude(a => a.Usuario)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => MapToDetailDto(s))
                .ToListAsync();
        }

        // 3. MERCADO (Asesor) - Con Filtro de Materia
        // Nota: Debes actualizar la interfaz ISolicitudService para aceptar el parámetro opcional
        public async Task<IEnumerable<SolicitudDetailDto>> GetSolicitudesDisponiblesAsync(string? materia = null)
        {
            var query = _context.SolicitudesDeAyuda
                .Where(s => s.Estado == "Abierta" && s.IsActive)
                .Include(s => s.Estudiante)
                .AsQueryable();

            if (!string.IsNullOrEmpty(materia))
            {
                query = query.Where(s => s.Materia.Contains(materia));
            }

            var solicitudes = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();

            // Solo mostramos detalle básico, sin ofertas de otros
            return solicitudes.Select(s => new SolicitudDetailDto
            {
                SolicitudId = s.SolicitudId,
                Materia = s.Materia,
                Tema = s.Tema,
                Descripcion = s.Descripcion,
                FechaLimite = s.FechaLimite,
                ArchivoUrl = s.ArchivoUrl,
                Estado = s.Estado,
                NombreEstudiante = s.Estudiante.UserName,
                FechaCreacion = s.CreatedAt,
                Ofertas = new List<OfertaDto>() // El asesor no ve ofertas de competencia aquí
            });
        }

        // 4. CREAR OFERTA (Asesor)
        public async Task<GenericResponseDto> CrearOfertaAsync(int solicitudId, OfertaCreateDto dto, int asesorId)
        {
            var solicitud = await _context.SolicitudesDeAyuda.FindAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != "Abierta")
                return new GenericResponseDto { IsSuccess = false, Message = "Solicitud no disponible." };

            // Validar si ya ofertó
            var yaOferto = await _context.OfertasSolicitud.AnyAsync(o => o.SolicitudId == solicitudId && o.AsesorId == asesorId);
            if (yaOferto) return new GenericResponseDto { IsSuccess = false, Message = "Ya enviaste una oferta." };

            var oferta = new OfertaSolicitud
            {
                SolicitudId = solicitudId,
                AsesorId = asesorId,
                PrecioOferta = dto.PrecioOferta,
                Mensaje = dto.Mensaje
            };

            await _context.OfertasSolicitud.AddAsync(oferta);
            await _context.SaveChangesAsync();

            // --- AUDITORÍA EN POSTGRES ---
            await RegistrarAuditoria(asesorId, "Enviar Oferta", $"Solicitud ID: {solicitudId}, Precio: {dto.PrecioOferta}");

            return new GenericResponseDto { IsSuccess = true, Message = "Oferta enviada." };
        }

        // 5. ACEPTAR OFERTA (Estudiante)
        public async Task<GenericResponseDto> AceptarOfertaAsync(int ofertaId, int estudianteId)
        {
            var oferta = await _context.OfertasSolicitud
                .Include(o => o.Solicitud).ThenInclude(s => s.Estudiante) // Incluir estudiante para obtener su nombre
                .Include(o => o.Asesor).ThenInclude(a => a.Usuario) // Incluir datos para el correo
                .FirstOrDefaultAsync(o => o.OfertaId == ofertaId);

            if (oferta == null) return new GenericResponseDto { IsSuccess = false, Message = "Oferta no encontrada." };

            // Validar propiedad
            if (oferta.Solicitud.EstudianteId != estudianteId)
                return new GenericResponseDto { IsSuccess = false, Message = "No autorizado." };

            if (oferta.Solicitud.Estado != "Abierta")
                return new GenericResponseDto { IsSuccess = false, Message = "Esta solicitud ya fue cerrada." };

            // Actualizar estado
            oferta.FueAceptada = true;
            oferta.Solicitud.Estado = "EnProceso";
            oferta.Solicitud.AsesorAsignadoId = oferta.AsesorId;
            await _context.SaveChangesAsync();

            // --- AUDITORÍA EN POSTGRES ---
            await RegistrarAuditoria(estudianteId, "Aceptar Oferta", $"Oferta ID: {ofertaId} del Asesor {oferta.AsesorId}");

            // --- ENVIAR CORREO AL ASESOR (DISEÑO MEJORADO) ---
            string emailAsesor = oferta.Asesor.Usuario.Email;
            string nombreAsesor = oferta.Asesor.Usuario.NombreCompleto ?? "Asesor";
            string nombreEstudiante = oferta.Solicitud.Estudiante?.UserName ?? "Estudiante";
            string linkDashboard = "http://localhost:5173/profile"; // Apunta al perfil

            string asunto = "¡Enhorabuena! Tu oferta ha sido aceptada - Lumina";

            string mensaje = $@"
                <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 10px; overflow: hidden; background-color: #ffffff;'>
                    <div style='background-color: #10b981; padding: 25px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 24px;'>¡Nueva Asesoría Confirmada!</h1>
                    </div>
                    <div style='padding: 30px;'>
                        <p style='font-size: 16px;'>Hola <strong>{nombreAsesor}</strong>,</p>
                        <p style='font-size: 16px; line-height: 1.5;'>
                            Buenas noticias. El estudiante ha aceptado tu propuesta para la solicitud sobre <strong>{oferta.Solicitud.Tema}</strong>.
                        </p>
                        
                        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 8px; margin: 25px 0; border-left: 5px solid #10b981;'>
                            <p style='margin: 5px 0;'><strong>Materia:</strong> {oferta.Solicitud.Materia}</p>
                            <p style='margin: 5px 0;'><strong>Precio Acordado:</strong> ${oferta.PrecioOferta}</p>
                            <p style='margin: 5px 0;'><strong>Estudiante:</strong> {nombreEstudiante}</p>
                        </div>

                        <p style='font-size: 16px; margin-bottom: 25px;'>
                            Es momento de ponerte en contacto. Accede a tu panel para ver los detalles completos y comenzar.
                        </p>

                        <div style='text-align: center;'>
                            <a href='{linkDashboard}' style='display: inline-block; background-color: #10b981; color: white; padding: 14px 28px; text-decoration: none; border-radius: 30px; font-weight: bold; font-size: 16px; box-shadow: 0 4px 6px rgba(16, 185, 129, 0.25);'>
                                Ir a Mis Asesorías
                            </a>
                        </div>
                    </div>
                    <div style='background-color: #f9fafb; padding: 15px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb;'>
                        &copy; 2025 Lumina Learning. Éxito en tu enseñanza.
                    </div>
                </div>
            ";

            _ = _emailService.SendEmailAsync(emailAsesor, asunto, mensaje);

            return new GenericResponseDto { IsSuccess = true, Message = "Oferta aceptada. Se ha notificado al asesor." };
        }

        // Helper de Mapeo
        private static SolicitudDetailDto MapToDetailDto(SolicitudDeAyuda s)
        {
            return new SolicitudDetailDto
            {
                SolicitudId = s.SolicitudId,
                Materia = s.Materia,
                Tema = s.Tema,
                Descripcion = s.Descripcion,
                FechaLimite = s.FechaLimite,
                ArchivoUrl = s.ArchivoUrl,
                Estado = s.Estado,
                NombreEstudiante = s.Estudiante?.UserName ?? "Usuario",
                FechaCreacion = s.CreatedAt,
                Ofertas = s.Ofertas?.Select(o => new OfertaDto
                {
                    OfertaId = o.OfertaId,
                    NombreAsesor = o.Asesor?.Usuario?.UserName ?? "Asesor",
                    Precio = o.PrecioOferta,
                    Mensaje = o.Mensaje,
                    FueAceptada = o.FueAceptada,
                    FechaOferta = o.CreatedAt
                }).ToList() ?? new List<OfertaDto>()
            };
        }

        // Helper de Auditoría
        private async Task RegistrarAuditoria(int userId, string accion, string detalles)
        {
            try
            {
                var log = new AuditoriaAccion
                {
                    UsuarioId = userId,
                    Accion = accion,
                    Detalles = detalles,
                    FechaAccion = DateTime.UtcNow
                };
                await _analyticsDb.Auditorias.AddAsync(log);
                await _analyticsDb.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando auditoría: {ex.Message}");
                // No lanzamos error para no detener el flujo principal
            }
        }

        public async Task<GenericResponseDto> UpdateSolicitudAsync(int solicitudId, SolicitudUpdateDto dto, int estudianteId)
        {
            var solicitud = await _context.SolicitudesDeAyuda.FindAsync(solicitudId);

            if (solicitud == null) return new GenericResponseDto { IsSuccess = false, Message = "No encontrada" };
            if (solicitud.EstudianteId != estudianteId) return new GenericResponseDto { IsSuccess = false, Message = "No autorizado" };
            if (solicitud.Estado != "Abierta") return new GenericResponseDto { IsSuccess = false, Message = "No se puede editar una solicitud en proceso o cerrada." };

            solicitud.Materia = dto.Materia;
            solicitud.Tema = dto.Tema;
            solicitud.Descripcion = dto.Descripcion;
            solicitud.FechaLimite = dto.FechaLimite;
            solicitud.ArchivoUrl = dto.ArchivoUrl;
            solicitud.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new GenericResponseDto { IsSuccess = true, Message = "Solicitud actualizada" };
        }

        public async Task<GenericResponseDto> DeleteSolicitudAsync(int solicitudId, int estudianteId)
        {
            var solicitud = await _context.SolicitudesDeAyuda.FindAsync(solicitudId);

            if (solicitud == null) return new GenericResponseDto { IsSuccess = false, Message = "No encontrada" };
            if (solicitud.EstudianteId != estudianteId) return new GenericResponseDto { IsSuccess = false, Message = "No autorizado" };

            // Soft Delete (El contexto lo maneja si configuramos IsActive)
            _context.SolicitudesDeAyuda.Remove(solicitud);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Solicitud eliminada" };
        }

        // 6. FINALIZAR Y CALIFICAR (Estudiante)
        public async Task<GenericResponseDto> FinalizarSolicitudAsync(int solicitudId, FinalizarSolicitudDto dto, int estudianteId)
        {
            var solicitud = await _context.SolicitudesDeAyuda
                .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId);

            if (solicitud == null) return new GenericResponseDto { IsSuccess = false, Message = "Solicitud no encontrada." };
            if (solicitud.EstudianteId != estudianteId) return new GenericResponseDto { IsSuccess = false, Message = "No autorizado." };
            if (solicitud.Estado != "EnProceso") return new GenericResponseDto { IsSuccess = false, Message = "Solo se pueden finalizar solicitudes en proceso." };

            // 1. Actualizar Estado en SQL Server
            solicitud.Estado = "Finalizada";
            solicitud.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 2. Guardar Calificación en PostgreSQL
            // Necesitamos el ID del asesor ganador
            if (solicitud.AsesorAsignadoId.HasValue)
            {
                var nuevaCalificacion = new Calificacion
                {
                    EstudianteId = estudianteId,
                    AsesorId = solicitud.AsesorAsignadoId.Value,
                    SolicitudId = solicitud.SolicitudId,
                    CursoId = null, // Es una asesoría, no un curso
                    Rating = dto.Rating,
                    Comentario = dto.Comentario,
                    CreatedAt = DateTime.UtcNow
                };
                await _analyticsDb.Calificaciones.AddAsync(nuevaCalificacion);
                await _analyticsDb.SaveChangesAsync();
            }

            return new GenericResponseDto { IsSuccess = true, Message = "Asesoría finalizada y calificada. ¡Gracias!" };
        }
    }
}