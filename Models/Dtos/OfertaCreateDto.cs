using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class OfertaCreateDto
    {
        [Required]
        [Range(0, 99999.99)]
        public decimal PrecioOferta { get; set; }

        [Required]
        [MaxLength(500)]
        public string Mensaje { get; set; }
    }
}
