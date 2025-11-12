using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class LeccionCreateDto
    {
        [Required(ErrorMessage = "El título de la lección es obligatorio")]
        [MaxLength(150)]
        public string Titulo { get; set; }

        [Required(ErrorMessage = "La URL del contenido es obligatoria")]
        [Url(ErrorMessage = "Debe ser una URL válida (ej. https://...)")]
        public string ContenidoUrl { get; set; }

        [Range(1, 1000, ErrorMessage = "El orden debe ser un número positivo")]
        public int Orden { get; set; } = 1;
    }
}
