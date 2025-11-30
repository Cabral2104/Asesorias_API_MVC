using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface ICursoService
    {
        // Tarea para que un asesor cree un nuevo curso
        Task<GenericResponseDto> CreateCursoAsync(CursoCreateDto dto, int asesorId);

        // Tarea para obtener todos los cursos públicos (para el catálogo)
        Task<IEnumerable<CursoPublicDto>> GetCursosPublicosAsync();

        // --- NUEVO MÉTODO ---
        // Tarea para que un asesor publique su curso
        Task<GenericResponseDto> PublishCursoAsync(int cursoId, int asesorId);

        // --- NUEVOS MÉTODOS ---

        // Tarea para que un asesor vea solo sus cursos
        Task<IEnumerable<CursoPublicDto>> GetMyCursosAsync(int asesorId);

        // Tarea para que un asesor actualice su curso
        // Reutilizamos el DTO de creación, ya que tiene los mismos campos
        Task<GenericResponseDto> UpdateCursoAsync(int cursoId, CursoCreateDto dto, int asesorId);

        Task<GenericResponseDto> DeleteCursoAsync(int cursoId, int asesorId);

        Task<CursoPublicDto?> GetCursoByIdForAsesorAsync(int cursoId, int asesorId);
    }
}
