using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface ISolicitudService
    {
        // Estudiante
        Task<GenericResponseDto> CrearSolicitudAsync(SolicitudCreateDto dto, int estudianteId);
        Task<IEnumerable<SolicitudDetailDto>> GetMisSolicitudesAsync(int estudianteId);
        Task<GenericResponseDto> AceptarOfertaAsync(int ofertaId, int estudianteId);

        // Asesor
        Task<IEnumerable<SolicitudDetailDto>> GetSolicitudesDisponiblesAsync();
        Task<GenericResponseDto> CrearOfertaAsync(int solicitudId, OfertaCreateDto dto, int asesorId);
    }
}
