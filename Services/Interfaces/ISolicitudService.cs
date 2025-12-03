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
        Task<IEnumerable<SolicitudDetailDto>> GetSolicitudesDisponiblesAsync(string? materia = null);
        Task<GenericResponseDto> CrearOfertaAsync(int solicitudId, OfertaCreateDto dto, int asesorId);

        Task<GenericResponseDto> UpdateSolicitudAsync(int solicitudId, SolicitudUpdateDto dto, int estudianteId);
        Task<GenericResponseDto> DeleteSolicitudAsync(int solicitudId, int estudianteId);
        Task<GenericResponseDto> FinalizarSolicitudAsync(int solicitudId, FinalizarSolicitudDto dto, int estudianteId);
    }
}
