using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class SolicitudCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string Materia { get; set; } // Nuevo

        [Required]
        [MaxLength(100)]
        public string Tema { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaLimite { get; set; } // Nuevo

        [Url]
        public string? ArchivoUrl { get; set; } // Nuevo (Link)
    }
}
