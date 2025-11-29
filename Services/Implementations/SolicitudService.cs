using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class SolicitudService : ISolicitudService
    {
        private readonly ApplicationDbContext _context;

        public SolicitudService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Estudiante crea solicitud
        public async Task<GenericResponseDto> CrearSolicitudAsync(SolicitudCreateDto dto, int estudianteId)
        {
            var solicitud = new SolicitudDeAyuda
            {
                EstudianteId = estudianteId,
                Tema = dto.Tema,
                Descripcion = dto.Descripcion,
                Estado = "Abierta"
                // CreatedAt se llena solo
            };

            await _context.SolicitudesDeAyuda.AddAsync(solicitud);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Solicitud publicada correctamente." };
        }

        // 2. Estudiante ve sus solicitudes y ofertas
        public async Task<IEnumerable<SolicitudDetailDto>> GetMisSolicitudesAsync(int estudianteId)
        {
            return await _context.SolicitudesDeAyuda
                .Where(s => s.EstudianteId == estudianteId && s.IsActive)
                .Include(s => s.Estudiante)
                .Include(s => s.Ofertas).ThenInclude(o => o.Asesor).ThenInclude(a => a.Usuario)
                .Select(s => MapToDetailDto(s))
                .ToListAsync();
        }

        // 3. Asesor ve solicitudes abiertas (Marketplace)
        public async Task<IEnumerable<SolicitudDetailDto>> GetSolicitudesDisponiblesAsync()
        {
            return await _context.SolicitudesDeAyuda
                .Where(s => s.Estado == "Abierta" && s.IsActive)
                .Include(s => s.Estudiante)
                .Include(s => s.Ofertas).ThenInclude(o => o.Asesor).ThenInclude(a => a.Usuario)
                .Select(s => MapToDetailDto(s))
                .ToListAsync();
        }

        // 4. Asesor envía cotización (Oferta)
        public async Task<GenericResponseDto> CrearOfertaAsync(int solicitudId, OfertaCreateDto dto, int asesorId)
        {
            // Validar existencia
            var solicitud = await _context.SolicitudesDeAyuda.FindAsync(solicitudId);
            if (solicitud == null || solicitud.Estado != "Abierta")
            {
                return new GenericResponseDto { IsSuccess = false, Message = "La solicitud no existe o ya fue cerrada." };
            }

            // Validar que sea asesor
            var asesor = await _context.Asesores.FindAsync(asesorId);
            if (asesor == null || !asesor.EstaAprobado)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Debes ser un asesor aprobado para ofertar." };
            }

            var oferta = new OfertaSolicitud
            {
                SolicitudId = solicitudId,
                AsesorId = asesorId,
                PrecioOferta = dto.PrecioOferta,
                Mensaje = dto.Mensaje
            };

            await _context.OfertasSolicitud.AddAsync(oferta);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Tu oferta ha sido enviada." };
        }

        // 5. Estudiante acepta oferta
        public async Task<GenericResponseDto> AceptarOfertaAsync(int ofertaId, int estudianteId)
        {
            var oferta = await _context.OfertasSolicitud
                .Include(o => o.Solicitud)
                .FirstOrDefaultAsync(o => o.OfertaId == ofertaId);

            if (oferta == null)
                return new GenericResponseDto { IsSuccess = false, Message = "Oferta no encontrada." };

            // Validar propiedad
            if (oferta.Solicitud.EstudianteId != estudianteId)
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permiso para aceptar esta oferta." };

            if (oferta.Solicitud.Estado != "Abierta")
                return new GenericResponseDto { IsSuccess = false, Message = "Esta solicitud ya está cerrada." };

            // Acciones
            oferta.FueAceptada = true;
            oferta.Solicitud.Estado = "EnProceso"; // Cierra la solicitud
            oferta.Solicitud.AsesorAsignadoId = oferta.AsesorId;

            await _context.SaveChangesAsync();
            return new GenericResponseDto { IsSuccess = true, Message = "¡Oferta aceptada! Ponte en contacto con tu asesor." };
        }

        // Helper
        private static SolicitudDetailDto MapToDetailDto(SolicitudDeAyuda s)
        {
            return new SolicitudDetailDto
            {
                SolicitudId = s.SolicitudId,
                Tema = s.Tema,
                Descripcion = s.Descripcion,
                Estado = s.Estado,
                NombreEstudiante = s.Estudiante?.UserName ?? "Usuario",
                FechaCreacion = s.CreatedAt,
                Ofertas = s.Ofertas.Select(o => new OfertaDto
                {
                    OfertaId = o.OfertaId,
                    NombreAsesor = o.Asesor?.Usuario?.UserName ?? "Asesor",
                    Precio = o.PrecioOferta,
                    Mensaje = o.Mensaje,
                    FueAceptada = o.FueAceptada,
                    FechaOferta = o.CreatedAt
                }).ToList()
            };
        }
    }
}
