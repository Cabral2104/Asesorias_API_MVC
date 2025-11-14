using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface ICalificacionService
    {
        Task<GenericResponseDto> AddCalificacionAsync(int cursoId, string estudianteId, CalificacionCreateDto dto);
    }
}
