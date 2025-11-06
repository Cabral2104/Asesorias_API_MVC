namespace Asesorias_API_MVC.Models.Dtos
{
    // Usaremos esto para todas las respuestas simples de la API
    public class GenericResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}

