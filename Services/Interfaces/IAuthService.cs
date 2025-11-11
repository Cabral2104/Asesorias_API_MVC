using Asesorias_API_MVC.Models.Dtos;

namespace Asesorias_API_MVC.Services.Interfaces
{
    public interface IAuthService
    {
        // Tarea que registra un nuevo usuario (como Estudiante)
        Task<AuthResponseDto> RegisterAsync(RegistroUsuarioDto registerDto);

        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    }
}
