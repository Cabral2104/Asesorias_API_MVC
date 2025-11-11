
namespace Asesorias_API_MVC.Models.Dtos
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
