using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface IEstudianteService
    {
        // Tarea para que un estudiante se inscriba a un curso
        Task<GenericResponseDto> InscribirseACursoAsync(InscripcionPagoDto pagoDto, int estudianteId);

        // Tarea para que un estudiante vea los cursos a los que está inscrito
        Task<IEnumerable<CursoPublicDto>> GetMisCursosAsync(int estudianteId);
    }
}
