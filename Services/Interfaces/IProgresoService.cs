using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface IProgresoService
    {
        Task<GenericResponseDto> MarcarLeccionAsync(int estudianteId, int leccionId, bool completada);
        Task<ProgresoCursoDto> ObtenerProgresoCursoAsync(int estudianteId, int cursoId);
    }
}
