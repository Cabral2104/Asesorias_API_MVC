using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<SolicitudAsesorDto>> GetPendingAsesorApplicationsAsync();
        Task<GenericResponseDto> ReviewAsesorApplicationAsync(string userId, bool approve);
    }
}
