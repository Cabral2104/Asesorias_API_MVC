using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface ILeccionService
    {
        // Obtener todas las lecciones de un curso
        Task<IEnumerable<LeccionPublicDto>> GetLeccionesByCursoIdAsync(int cursoId);

        // Agregar una lección a un curso (solo el dueño)
        Task<GenericResponseDto> AddLeccionToCursoAsync(int cursoId, LeccionCreateDto dto, string asesorId);

        // Actualizar una lección (solo el dueño)
        Task<GenericResponseDto> UpdateLeccionAsync(int leccionId, LeccionCreateDto dto, string asesorId);

        // Eliminar una lección (solo el dueño)
        Task<GenericResponseDto> DeleteLeccionAsync(int leccionId, string asesorId);
    }
}
