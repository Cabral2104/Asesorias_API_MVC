using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface ICursoService
    {
        // Tarea para que un asesor cree un nuevo curso
        Task<GenericResponseDto> CreateCursoAsync(CursoCreateDto dto, string asesorId);

        // Tarea para obtener todos los cursos públicos (para el catálogo)
        Task<IEnumerable<CursoPublicDto>> GetCursosPublicosAsync();

        // --- NUEVO MÉTODO ---
        // Tarea para que un asesor publique su curso
        Task<GenericResponseDto> PublishCursoAsync(int cursoId, string asesorId);

        // --- NUEVOS MÉTODOS ---

        // Tarea para que un asesor vea solo sus cursos
        Task<IEnumerable<CursoPublicDto>> GetMyCursosAsync(string asesorId);

        // Tarea para que un asesor actualice su curso
        // Reutilizamos el DTO de creación, ya que tiene los mismos campos
        Task<GenericResponseDto> UpdateCursoAsync(int cursoId, CursoCreateDto dto, string asesorId);

        Task<GenericResponseDto> DeleteCursoAsync(int cursoId, string asesorId);
    }
}
