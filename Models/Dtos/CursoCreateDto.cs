using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class CursoCreateDto
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(150, ErrorMessage = "El título no puede exceder 150 caracteres")]
        public string Titulo { get; set; }

        [MaxLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "El costo es obligatorio")]
        [Range(0, 99999.99, ErrorMessage = "El costo debe ser un valor positivo")]
        public decimal Costo { get; set; }
    }
}
