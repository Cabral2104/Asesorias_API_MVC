using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class CalificacionCreateDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5.")]
        public int Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres.")]
        public string Comentario { get; set; }
    }
}
