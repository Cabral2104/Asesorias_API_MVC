using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface ISolicitudService
    {
        // Estudiante
        Task<GenericResponseDto> CrearSolicitudAsync(SolicitudCreateDto dto, string estudianteId);
        Task<IEnumerable<SolicitudDetailDto>> GetMisSolicitudesAsync(string estudianteId);
        Task<GenericResponseDto> AceptarOfertaAsync(int ofertaId, string estudianteId);

        // Asesor
        Task<IEnumerable<SolicitudDetailDto>> GetSolicitudesDisponiblesAsync();
        Task<GenericResponseDto> CrearOfertaAsync(int solicitudId, OfertaCreateDto dto, string asesorId);
    }
}
