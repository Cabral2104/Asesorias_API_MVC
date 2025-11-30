using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface IAsesorService
    {
        // Tarea para solicitar ser asesor
        Task<GenericResponseDto> ApplyToBeAsesorAsync(AsesorApplyDto dto, int userId);

        Task<IEnumerable<ChartStatDto>> GetActivityChartAsync(int asesorId);
    }
}
